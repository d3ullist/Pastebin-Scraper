using Scrapers;
using System;
using System.IO;
using System.Xml.Serialization;

public class ScrapperStatistics
{
    public int TotalScraped { get; set; }
    public int TotalFaulty { get; set; }
    public int TotalDownloaded { get; set; }

    public int EmailsFound { get; set; }
    public int CombolistsFound { get; set; }
    public int FilesContainingEmails { get; set; }
    public int FilesContainingCombolists { get; set; }

    public int LastScrapeAmount { get; set; }

    // Empty constructor
    public ScrapperStatistics() { }
}

public static class Stats
{
    public static ScrapperStatistics ScraperStats = new ScrapperStatistics();

    public static void SaveStats()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ScrapperStatistics));
            // TODO: Create folder if does not exist
            using(TextWriter writer = new StreamWriter(string.Format(@"{0}\Output\stats.xml", Pastebin.SaveLocation)))
            {
                serializer.Serialize(writer, Stats.ScraperStats);
            }
        }
        catch
        {
            return;
        }
    }

    public static void LoadStats()
    {
        if (!File.Exists(Pastebin.SaveLocation + @"\Output\stats.xml"))
        {
            Console.WriteLine("No Data to load (stats)");
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(ScrapperStatistics));
        try
        {
            using (StreamReader stream = new StreamReader(string.Format(@"{0}\Output\stats.xml", Pastebin.SaveLocation)))
            {
                ScraperStats = (ScrapperStatistics)serializer.Deserialize(stream);
            }
        }
        catch
        {
            Console.WriteLine("Failed to download data (stats)");
            return;
        }
    }
}
