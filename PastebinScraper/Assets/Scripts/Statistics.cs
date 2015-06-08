using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statistics
{
    public int TotalFiles = 0;
    public int TotalEmails = 0;
    public int TotalDuplicates = 0;

    public Dictionary<string, int> Dupes = new Dictionary<string, int>();

    public int TotalDupes()
    {
        var total = 0;

        foreach (KeyValuePair<string, int> pair in Dupes)
        {
            total += pair.Value;
        }

        return total;
    }
}
