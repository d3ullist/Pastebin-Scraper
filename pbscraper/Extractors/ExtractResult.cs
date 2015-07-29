using Scrapers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class Extract
    {
        public static void Save(ExtractResult ER, string FileSpec)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ExtractResult));
            TextWriter tw = new StreamWriter(@"C:\Users\D3ullist\Desktop\Pastes\tester.xml");
            xs.Serialize(tw, ER);
        }
        public static void Load()
        {
            //using (var sr = new StreamReader(@"c:\temp\garage.xml"))
            //{
            //    garage = (theGarage)xs.Deserialize(sr);
            //}
        }

        public void Test()
        {
            ExtractResult ee = new ExtractResult(
                "Lore",
                "DOLAS",
                2819,
                new List<string>() { "asdf","asdfasdf","asdfasd"});
            //ExtractResult ea = new ExtractResult("Lore", "ipsum", 2819);


            Extract.Save(ee, "Test.xml");
            //Extract.Save(ea, "TestTwo.xml");
        }
    }
}
