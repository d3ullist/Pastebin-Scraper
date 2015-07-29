using Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Scrapers
{
    public class Pastebin
    {
        const String PastebinArchiveLink = "http://pastebin.com/archive";

        const int Cooldown = 90;
        const int MaxPerCooldown = 75;

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
            foreach (LinkItem LI in LinkFinder.Find(PastebinArchive()))
            {
                string sHref = StripLinkItem(LI.Href);

                // If the stripd down link is excluded, or irrelevant go to the next.
                if (string.IsNullOrEmpty(sHref)) continue;

                // If we did not previously handle this, add it to our current.
                if (previous.Add(sHref)) current.Add(sHref);
                else faulty.Add(sHref);
            }

            DownloadPastes(ref current);
        }

        public void DownloadPastes(ref HashSet<string> IDs)
        {
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        faulty.Add(ID);
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
