using Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace Scraper
{
    public class Pastebin
    {
        private const string PASTEBIN_ARCHIVE = "http://pastebin.com/archive";
        private HashSet<string> linkExclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "archive",
            "trends",
            "signup",
            "alerts",
            "settings",
            "profile",
            "signup",
            "privacy",
            "contact",
            "languages"
        };

        public bool isInitialized = false;
        private string saveLocation = string.Empty;
        private string executingAssembly = string.Empty;
        private HashSet<string> storedPastes = new HashSet<string>();
        private HashSet<string> newUrls = new HashSet<string>();

        public string SaveLocation
        {
            get 
            {
                if (string.IsNullOrEmpty(saveLocation))
                {
                    saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Pastes";
                    Initializer();
                }
                return saveLocation; 
            }
            set { saveLocation = value; }
        }

        private void Initializer()
        {
            // Check if the set save location exists
            if (!Directory.Exists(SaveLocation))
            {
                Debug.Log("Save location did not exist");
                try
                {
                    Directory.CreateDirectory(SaveLocation);
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to create Directory: " + e);
                    return;
                }
            }

            // Get executing assembly
            if (string.IsNullOrEmpty(executingAssembly))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                executingAssembly = Uri.UnescapeDataString(uri.Path);
            }

            // Get the last received list
            //if (!string.IsNullOrEmpty(executingAssembly))
            //{
            //    var parent = Directory.GetParent(executingAssembly);
            //    if (!File.Exists(parent + @"\ReceivedPastes.txt"))
            //    {
            //        // empty using to create file, so we don't have to clean up behind ourselfs.
            //        using (FileStream fs = new FileStream(parent + @"\ReceivedPastes.txt", FileMode.CreateNew)) { }
            //    }
            //    else
            //    {
            //        using (FileStream fs = new FileStream(parent + @"\ReceivedPastes.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //        {
            //            if (fs.Length > 20000)
            //                fs.Seek(-20000, SeekOrigin.End);

            //            using (StreamReader sr = new StreamReader(fs))
            //            {
            //                while (sr.ReadLine() != null)
            //                {
            //                    storedPastes.Add(sr.ReadLine());
            //                }
            //            }
            //        }
            //    }
            //}

            isInitialized = true;
        }

        public void Scrape()
        {
            var response = string.Empty;

            if (!isInitialized)
            {
                Initializer();
                if (!isInitialized)
                {
                    Debug.Log("Not correctly initialized, trying again on next call");
                    return;
                }
            }

            using (WebClient wc = new WebClient())
            {
                // Download the current archive page
                try { response = wc.DownloadString(PASTEBIN_ARCHIVE); }
                catch (Exception e)
                {
                    Debug.Log("Failed to download archive: " + e);
                    return;
                }

                // Loop through the links found in this page and exclude links we don't need
                foreach (LinkItem li in LinkFinder.Find(response))
                {
                    if (li.Href.StartsWith("/") && li.Href.Length < 11 && li.Href.Length > 6 && !linkExclude.Contains(li.Href.Remove(0, 1))
                        && !storedPastes.Contains(li.Href.Remove(0, 1)))
                        newUrls.Add(li.Href.Remove(0, 1));
                }
            }

            DownloadPastes();
        }

        private void DownloadPastes()
        {
            HashSet<string> toRemove = new HashSet<string>();

            foreach (string url in newUrls)
            {
                var downloadUrl = new Uri("http://pastebin.com/raw.php?i=" + url);
                // TODO: Add to total files

                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        wc.DownloadFileAsync(downloadUrl, saveLocation + @"\" + url + ".txt");
                        storedPastes.Add(url);
                        toRemove.Add(url);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(string.Format("Failed to download {0} Keeping it in urls for next round: {1}", url, e));
                    }
                }
            }
            foreach (string url in toRemove)
            {
                newUrls.Remove(url);
            }
        }
    }
}