using Extractors;
using FileManager;
using Scrapers;
using System;

namespace pbscraper
{
    class Program
    {
        public const string Version = "Version: 0.1.3";
        public const int MSSecond = 1000;
        public const int MSMinute = MSSecond * 60;
        public const int MSHour = MSMinute * 60;

        static System.Timers.Timer timer1 = new System.Timers.Timer();
        static Pastebin pb = new Pastebin();
        static void Main(string[] args)
        {
            Stats.LoadStats();

            String command;
            Boolean quitNow = false;
            Boolean scrape = false;

            Console.WriteLine("Pastebin scrapper and proccesor " + Version);
            Console.WriteLine("Supported commands: /pastebin /version /test /clear");

            while (!quitNow)
            {
                command = Console.ReadLine();

                switch (command.ToLower())
                {
                    case "/pastebin":
                        Console.WriteLine("Pastebin state: " + !scrape);
                        if (scrape = !scrape == true) StartTimer();
                        else StopTimer();
                        break;
                    case "/stats": WriteStats(); break;
                    case "/test": break;
                    case "/version": Console.WriteLine(Version); break;
                    case "/merge":
                        Merger me = new Merger();
                        me.MergeFilesAsync(51200, Pastebin.SaveLocation + @"\Output");
                        break;
                    case "/extract email":
                        Email e = new Email();
                        e.ExtractEmailsAsync();
                        break;
                    case "/result email": WriteEmailResult(); break;
                    case "/quit":
                        quitNow = true;
                        Stats.SaveStats();
                        break;
                    case "/help":
                    case "/?":
                        //todo write help message
                        break;
                    case "/clear":
                        Console.Clear();
                        break;
                    case "/stats reset":
                        Stats.ScraperStats = new ScrapperStatistics();
                        break;
                    default:
                        Console.WriteLine("Unknown Command " + command);
                        break;
                }
            }
        }

        static void StartTimer()
        {
            timer1.Interval = MSMinute * 1.5; // 1.5 minutes
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Start();

            // Initial scrape
            pb.Scrape();

            Console.WriteLine(string.Format("TS: {0} - TF: {1} - TD: {2} - LSA: {3} - TI: {4}", Stats.ScraperStats.TotalScraped, Stats.ScraperStats.TotalFaulty, Stats.ScraperStats.TotalDownloaded, Stats.ScraperStats.LastScrapeAmount, timer1.Interval));

            // Merge small files
            // Merger m = new Merger();
            // m.MergeFilesAsync(15360, Pastebin.SaveLocation);
        }
        static void StopTimer()
        {
            timer1.Elapsed -= new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Stop();
        }

        static void WriteEmailResult()
        {
            int op = 0;
            string input = string.Empty;

            if (Extract.EmailResults.Count > 0)
            {
                Console.WriteLine("Currently " + Extract.EmailResults.Count + " Available, pick a number from 0 to " + (Extract.EmailResults.Count - 1) + " for more details ");
                do
                {
                    Console.WriteLine("enter choice");
                    input = Console.ReadLine();
                } while (!int.TryParse(input, out op));

                ExtractResult er = Extract.EmailResults[op];

                Console.WriteLine("Full path: " + er.filePath);
                Console.WriteLine("File Name: " + er.fileName);
                Console.WriteLine("Email Count: " + er.totalHits);
                Console.WriteLine("Do you wish to see all results? Y/N");
                string command = Console.ReadLine();

                if (command.ToLower() == "y")
                {
                    foreach (string a in er.emails)
                    {
                        Console.WriteLine(a);
                    }
                }
            }
            else
            {
                Console.WriteLine("No email results to review, please make sure you ran the extracter before (/extract email)");
            }
        }
        static void WriteStats()
        {
            Console.WriteLine(string.Format("TS: {0} - TF: {1} - TD: {2} - LSA: {3} - TI: {4}",
                Stats.ScraperStats.TotalScraped,
                Stats.ScraperStats.TotalFaulty,
                Stats.ScraperStats.TotalDownloaded,
                Stats.ScraperStats.LastScrapeAmount,
                timer1.Interval));
        }

        static private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            pb.Scrape();

            int last = Stats.ScraperStats.LastScrapeAmount;

            // Dynamic update of scraping interval, based on the last result
            if (last >= 20)
            { // Active, set to 1 minute
                timer1.Interval = MSMinute;
            }
            else if (last >= 13 && timer1.Interval > MSMinute)
            { // Above minimum and above 1 minute. -15 seconds
                timer1.Interval -= MSSecond * 15;
            }
            else if (last >= 3 && timer1.Interval < MSMinute * 5)
            { // Below minimum and under 5 minute. +15 seconds
                timer1.Interval += MSSecond * 15;
            }
            else if (last >= 1 && timer1.Interval < MSMinute * 5)
            { // Low and under 5 minutes. +30 seconds
                timer1.Interval += MSSecond * 30;
            }
            else if (last == 0 && timer1.Interval < MSMinute * 2)
            { // Zero and under 2 minutes. Set to 2 minutes.
                timer1.Interval = MSMinute * 2;
            }
            else if (last == 0 && timer1.Interval <= MSMinute * 5)
            { // Zero when already 2 minutes. +30 seconds.
                timer1.Interval = timer1.Interval + MSSecond * 30;
            }

            Console.WriteLine(string.Format("TS: {0} - TF: {1} - TD: {2} - LSA: {3} - TI: {4}", Stats.ScraperStats.TotalScraped, Stats.ScraperStats.TotalFaulty, Stats.ScraperStats.TotalDownloaded, Stats.ScraperStats.LastScrapeAmount, timer1.Interval));

        }
    }
}
