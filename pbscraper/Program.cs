using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrapers;
using FileManager;
using Extractors;

namespace pbscraper
{
    class Program
    {
        static System.Timers.Timer timer1 = new System.Timers.Timer();
        static Pastebin pb = new Pastebin();
        static void Main(string[] args)
        {
            String command;
            Boolean quitNow = false;
            Boolean scrape = false;

            while (!quitNow)
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "/pastebin":
                        Console.WriteLine("Pastebin state: " + !scrape);
                        if (scrape = !scrape == true) StartTimer();
                        else StopTimer();
                        break;
                    case "/test":
                        Extract ex = new Extract();
                        ex.Test();
                        break;
                    case "/version":
                        Console.WriteLine("This should be version.");
                        break;
                    case "/quit":
                        quitNow = true;
                        break;
                    case "/help":
                    case "/?":
                        //todo write help message
                        break;

                    default:
                        Console.WriteLine("Unknown Command " + command);
                        break;
                }
            }
        }

        static void StartTimer()
        {
            timer1.Interval = 90000; //1.5 minutes
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Start();

            // Initial scrape
            pb.Scrape();

            // Merge small files
            Merger m = new Merger();
            m.MergeFilesAsync(15360, Pastebin.SaveLocation);
        }
        static void StopTimer()
        {
            timer1.Elapsed -= new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Stop();
        }
        static private void timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            pb.Scrape();
        }
    }
}
