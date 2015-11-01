using Html;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Scrapers
{
    public class Pastebin
    {
        public Pastebin(int threads)
        {
            Task.Factory.StartNew(() => PbScrape(threads), TaskCreationOptions.LongRunning);
        }
        private void PbScrape(int threads)
        {
            // No buffering, supports low latency, proccesed as soon as source available.
            var src = Partitioner.Create(_urls.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
        
            // Start scraping using the xxx set of threads, will block thread until available.
            Parallel.ForEach(src, new ParallelOptions { MaxDegreeOfParallelism = threads }, PbScrape); 
        }
        private void PbScrape(Uri uri)
        {
            using (WebClient WC = new WebClient())
            {
                try
                {
                    var ID = uri.ToString().Substring(uri.ToString().IndexOf('=') + 1);
                    // TODO: Allow variable file naming (titles maybe?)
                    WC.DownloadFileAsync(uri, SaveLocation + @"\" + ID + ".txt");
                    Stats.ScraperStats.TotalDownloaded++;
                    Stats.ScraperStats.LastScrapeAmount++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Stats.ScraperStats.TotalFaulty++;
                }
            }
        }
        public void Start()
        {
            try
            {
                // Foreach link we find on the pastebinarchive page
                // TODO: replace this with a htmlagility pack call
                foreach (LinkItem LI in LinkFinder.Find(PastebinArchive()))
                {
                    string sHref = StripLinkItem(LI.Href);

                    // If the stripd down link is excluded, or irrelevant go to the next.
                    if (string.IsNullOrEmpty(sHref) || exclusionList.Contains(sHref)) continue;

                    // Check if we proccesed the link before
                    if (previous.Add(sHref))
                    {
                        _urls.Add(new Uri("http://pastebin.com/raw.php?i=" + sHref));
                        Stats.ScraperStats.TotalScraped++;
                    }
                    else
                    {
                        faulty.Add(sHref);
                        Stats.ScraperStats.TotalFaulty++;
                    }
                }
                // Exclude the 8 faulty caused by the 8 featured files
                Stats.ScraperStats.TotalFaulty -= 8;
            }
            catch
            {
                // Implement 3 strike out system
                Console.WriteLine("Could not download archive, due to a time out");
            }
        }
        private readonly HtmlWeb _web = new HtmlWeb();
        private readonly BlockingCollection<Uri> _urls = new BlockingCollection<Uri>();
        public int QueueLength
        {
            get { return _urls.Count; }
        }

        const String PastebinArchiveLink = "http://pastebin.com/archive";

        const int Cooldown = 90;

        HashSet<string> exclusionList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tools", "login", "archive", "trends", "signup", "alerts", "settings", "profile", "signup", "privacy", "contact", "languages" };
        HashSet<string> previous = new HashSet<string>();
        HashSet<string> current = new HashSet<string>();
        HashSet<string> faulty = new HashSet<string>();

        static string saveLocation = string.Empty;
        public static String SaveLocation
        {
            get
            {
                if (String.IsNullOrEmpty(saveLocation))
                {
                    // Set a default save location
                    saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Pastes";

                    // Check if it exists, or create it
                    if (!Directory.Exists(saveLocation))
                    {
                        try
                        {
                            Directory.CreateDirectory(saveLocation);
                            return saveLocation;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to create directory: " + e);
                            return null;
                        }
                    }
                    return saveLocation;
                }
                else
                {
                    // Check if it exists, or create it
                    if (!Directory.Exists(saveLocation))
                    {
                        try
                        {
                            Directory.CreateDirectory(saveLocation);
                            return saveLocation;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to create directory: " + e);
                            return null;
                        }
                    }
                }
                return saveLocation;
            }
            set { saveLocation = value; }
        }

        public void Scrape()
        {
            try
            {
                foreach (LinkItem LI in LinkFinder.Find(PastebinArchive()))
                {
                    string sHref = StripLinkItem(LI.Href);

                    // If the stripd down link is excluded, or irrelevant go to the next.
                    if (string.IsNullOrEmpty(sHref) || exclusionList.Contains(sHref)) continue;

                    // If we did not previously handle this, add it to our current.
                    if (previous.Add(sHref))
                    {
                        current.Add(sHref);
                        Stats.ScraperStats.TotalScraped++;
                    }
                    else
                    {
                        faulty.Add(sHref);
                        Stats.ScraperStats.TotalFaulty++;
                    }
                }
                // Exclude the 8 faulty caused by the 8 featured files
                Stats.ScraperStats.TotalFaulty -= 8;

                DownloadPastes(ref current);
            }
            catch
            {
                Console.WriteLine("Could not download archive, due to a time out");
            }
        }

        public void DownloadPastes(ref HashSet<string> IDs)
        {
            Stats.ScraperStats.LastScrapeAmount = 0; // Wipe the scrape amount of last run

            foreach (string ID in IDs)
            {
                // create a uri with the given id
                Uri downloadLink = new Uri("http://pastebin.com/raw.php?i=" + ID);

                using (WebClient WC = new WebClient())
                {
                    try
                    {
                        // TODO: Allow variable file naming (titles maybe?)
                        WC.DownloadFileAsync(downloadLink, SaveLocation + @"\" + ID + ".txt");
                        Stats.ScraperStats.TotalDownloaded++;
                        Stats.ScraperStats.LastScrapeAmount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        faulty.Add(ID);
                        Stats.ScraperStats.TotalFaulty++;
                    }
                }
            }

            previous.Clear(); // Clean previous
            previous = faulty; // Add faulty to previous
            previous.UnionWith(current); // Perform union with current
            current.Clear(); faulty.Clear(); // Wipe current and faulty
        }

        public string PastebinArchive()
        {
            using (WebClient wc = new WebClient())
            {
                string response = string.Empty;

                // Attempt to download the archive page
                try
                {
                    response = wc.DownloadString(PastebinArchiveLink);
                    if (!string.IsNullOrEmpty(response)) return response;
                    else return string.Empty;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to download archive: " + e);
                    return string.Empty;
                }
            }
        }
        public string StripLinkItem(string item)
        {
            if (!item.StartsWith("/")) return null;
            if (item.Length > 11 || item.Length < 6) return null;

            string sli = item.Remove(0, 1);

            if (exclusionList.Contains(sli)) return null;

            return sli;
        }
    }
}
