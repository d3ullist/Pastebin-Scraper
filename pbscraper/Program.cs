using Extractors;
using FileManager;
using Scrapers;
using System;
using System.Timers;

namespace pbscraper
{
    class Program
    {
        public const string Version = "Version: 0.1.4";
        public const int MSSecond = 1000;
        public const int MSMinute = MSSecond * 60;
        public const int MSHour = MSMinute * 60;

        static System.Timers.Timer timer1 = new System.Timers.Timer();
        static Pastebin pb = new Pastebin(Environment.ProcessorCount);
        static void Main(string[] args)
        {
            Stats.LoadStats();

            String command;

            Boolean quitNow = false;
            Console.WriteLine("Pastebin scrapper and proccesor " + Version);
            Console.WriteLine("Supported commands: /pastebin /version /test /clear");

            while (!quitNow)
            {
                command = Console.ReadLine();

                switch (command.ToLower())
                {
                    case "/pastebin": TriggerScraper(); break;
                    case "/stats": WriteStats(); break;
                    case "/test": break;
                    case "/version": Console.WriteLine(Version); break;
                    case "/merge":
                        me.MergeFilesAsync(sizeInBytes, Pastebin.SaveLocation);
                        break;
                    case "/merge size":
                        // TODO: Size assignment + handling (kb/mb etc)
                        break;
                    case "/merge auto":
                        // TODO: Handle automatic merging after xxx iterations OR xxx files
                        break;
                    case "/extract email":
                        Email e = new Email();
                        // TODO: Determine how the files should be handled
                        e.ExtractEmailsAsync();
                        break;
                    case "/result email": WriteEmailResult(); break;
                    case "/quit":
                        quitNow = true;
                        Stats.SaveStats();
                        break;
                    case "/help":
                    case "/?":
                        // TODO: write help message
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

        static Boolean scrape = false;
        static void TriggerScraper()
        {
            Console.WriteLine("Pastebin state: " + !scrape);

            if (scrape = !scrape == true) 
                StartTimer();
            else StopTimer();
        }

        static void StartTimer()
        {
            timer1.Interval = MSMinute * 1.5; // 1.5 minutes
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Start();

            // Initial scrape
            //pb.Scrape();
            pb.Start();

            Console.WriteLine(string.Format("TS: {0} - TF: {1} - TD: {2} - LSA: {3} - TI: {4}", Stats.ScraperStats.TotalScraped, Stats.ScraperStats.TotalFaulty, Stats.ScraperStats.TotalDownloaded, Stats.ScraperStats.LastScrapeAmount, timer1.Interval));
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

        static void ExctractEmailSettings()
        {
            bool valid = false;

            Console.WriteLine("First time Email Extracting setup");
            Console.WriteLine("After initial setup you wil no longer be prompted with these questions");
            Console.WriteLine("All settings can be altered at a later time using the /extract email settings command");

            Console.WriteLine("Do you wish to set the files aside after identification Y/N?");
            Console.WriteLine("This will prevent /merge and /clean from removing the files");

            do
            {
               string input = Console.ReadLine();
               if (input.ToLower() == "y")
               {
                   protectExtractedFiles = true;
                   valid = true;
               }
               else if (input.ToLower() == "n")
               {
                   protectExtractedFiles = false;
                   valid = true;
               }
            } while (!valid);

            valid = false;

            Console.WriteLine("Do you want all emails to be compiled into 1 big list? Y/N");
            Console.WriteLine("Otherwise results will be stored alongside their file in a path/name/resutls format");

            do
            {
                string input = Console.ReadLine();
                if (input.ToLower() == "y")
                {
                    forceOneList = true;
                    valid = true;
                }
                else if (input.ToLower() == "n")
                {
                    forceOneList = false;
                    valid = true;
                }
            } while (!valid);

            valid = false;

            Console.WriteLine("Would you like to remove the small files before extracting? Y/N");
            Console.WriteLine("By default all files under 10 characters will be removed");
            Console.WriteLine("This can be changed at any time in the settings menu");
        }

        static Boolean cleanFilesUnderSize = false;
        static Boolean protectExtractedFiles = false;
        static Boolean forceOneList = false;
        static Boolean extractEmails = false;
        static Boolean mergeFiles = false;

        static Merger me = new Merger();
        static string saveLocation = null;
        static int sizeInBytes = 51200;

        static private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            // On Elapse, scrape the next set of 25 (50 if premium)
            //pb.Scrape();
            pb.Start();

            // If enabled, clean files under a specific size
            if (cleanFilesUnderSize == true)
            {
                // TODO: Remove files under a xxx size to speed up procces
            }

            // If automatic email extraction is activated, and not already running
            // TODO: Add a check to see if we are already running (bussy) 
            if (extractEmails == true)
            {

            }

            // If files under a certain size should be merged, do so.
            // Default save location: Desktop/Pastes/Output
            // Default size < 50kb
            if (mergeFiles == true)
            {
                if (saveLocation == null) saveLocation = Pastebin.SaveLocation + @"\Output";

                me.MergeFilesAsync(sizeInBytes, saveLocation);
            }

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
