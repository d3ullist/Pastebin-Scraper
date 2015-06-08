using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Log
{
    //public static void Mail(HashSet<string> emails, string origin, bool containsComboList)
    //{
    //    if (string.IsNullOrEmpty(Extractors.E_SaveLocation)) Extractors.E_SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Pastes\EmailList.txt";
        
    //    using (StreamWriter sw = File.AppendText(Extractors.E_SaveLocation))
    //    {
    //        sw.WriteLine(string.Format("File name: {0} | amount: {1} | ContainsCombo's {2}", origin, emails.Count, containsComboList));
    //        sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    //        foreach (string email in emails)
    //        {
    //            sw.WriteLine(email);
    //        }
    //        sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    //        sw.WriteLine("");

    //        sw.Flush();
    //        sw.Close();
    //    }
    //}

    public static void ComboList(HashSet<string> combos, string origin)
    {
        using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Pastes\ComboList.txt"))
        {
            sw.WriteLine(origin);
            sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (string combo in combos)
            {
                string result = Regex.Replace(combo, @"\r\n?|\n", "");
                sw.WriteLine(result);
            }
            sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            sw.WriteLine("");

            sw.Flush();
            sw.Close();
        }
    }
}
