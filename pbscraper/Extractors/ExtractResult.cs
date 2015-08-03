using Scrapers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Extractors
{
    [System.Serializable]
    public class ExtractResult
    {
        public string fileName;
        public string filePath;

        public long totalHits;

        public List<string> emails = new List<string>();
        public KeyValuePair<string, string> emailPasswordCombo;

        public ExtractResult() { }
        public ExtractResult(string name, string path, long hits)
        {
            fileName = name;
            filePath = path;
            totalHits = hits;
        }
        public ExtractResult(string name, string path, long hits, List<string> email)
        {
            fileName = name;
            filePath = path;
            totalHits = hits;
            emails = email;
        }

    }

    public static  class Extract
    {
        public static List<ExtractResult> EmailResults = new List<ExtractResult>();
        public static List<ExtractResult> ComboResults = new List<ExtractResult>();

        public static void SaveStats()
        {
            //try
            //{
            //    XmlSerializer serializer = new XmlSerializer(typeof(ScrapperStatistics));
            //    using (TextWriter writer = new StreamWriter(string.Format(@"{0}\Output\stats.xml", Pastebin.SaveLocation)))
            //    {
            //        serializer.Serialize(writer, Stats.ScraperStats);
            //    }
            //}
            //catch
            //{
            //    throw;
            //}
        }

        //public static void LoadStats()
        //{
        //    if (!File.Exists(Pastebin.SaveLocation + @"\Output\stats.xml"))
        //    {
        //        Console.WriteLine("No Data to load (stats)");
        //        return;
        //    }

        //    XmlSerializer serializer = new XmlSerializer(typeof(ScrapperStatistics));
        //    try
        //    {
        //        using (StreamReader stream = new StreamReader(string.Format(@"{0}\Output\stats.xml", Pastebin.SaveLocation)))
        //        {
        //            ScraperStats = (ScrapperStatistics)serializer.Deserialize(stream);
        //        }
        //    }
        //    catch
        //    {
        //        Console.WriteLine("Failed to download data (stats)");
        //        return;
        //    }
        //}
    }
}
