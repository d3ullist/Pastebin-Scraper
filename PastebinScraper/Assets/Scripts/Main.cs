using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scraper;
using System;
using Extractors;

public partial class Main : MonoBehaviour 
{
    public bool IsScrapingPastebin = false;
    public bool IsDeletingSmallFiles = false;
    public bool IsExtractingEmails = false;

    public int AmountOfChars = 10; // Lower limit for deleting text files.

    private string nextScrape = string.Empty;

    private Pastebin pastebin = new Pastebin();
    private Email ee = new Email();
    public static Statistics stats = new Statistics();
    private FileCleaner fc = new FileCleaner();

	IEnumerator Start () 
    {
        Debug.Log("Application awake, starting co-Update");
        StartCoroutine(CoUpdate());

        while (Application.isPlaying)
        {
            if (IsScrapingPastebin) pastebin.Scrape();

            // Setting next estimate scrape time, and wait for 5 minutes
            int index = DateTime.Now.AddMinutes(5).TimeOfDay.ToString().IndexOf(".");
            if (index > 0)
                nextScrape = DateTime.Now.AddMinutes(5).TimeOfDay.ToString().Substring(0, index);
            yield return new WaitForSeconds(300);
        }
	}
    IEnumerator CoUpdate()
    {
        Debug.Log("Co-Update is running");
        while (Application.isPlaying)
        {
            #region EmailExtraction
            if (IsExtractingEmails && !ee.IsBusy)
            {
                Debug.Log("Starting up a email extraction Unit");
                ee.ExtractAsync(pastebin.SaveLocation, pastebin.SaveLocation, true);
                Debug.Log("Email Extraction Now Running");
            }
            else if (!IsExtractingEmails && ee.IsBusy)
            {
                Debug.Log("Terminating email extraction Unit");
                ee.Cancel();
                while (ee.IsBusy) {yield return new WaitForSeconds(1);}
                Debug.Log("Terminated email extraction Unit");
            }
            #endregion
            #region SmallFileCleaner
            if (IsDeletingSmallFiles && !fc.IsBusy && !fc.completed)
            {
                Debug.Log("Starting up a small file cleaner unit");
                fc.RemoveUnderAsync(10, pastebin.SaveLocation);
                Debug.Log("Started up a small file cleaner unit");
            }
            else if (!IsDeletingSmallFiles && fc.IsBusy)
            {
                Debug.Log("Terminating small file cleaner unit");
                fc.terminateWorker = true;
                while (fc.IsBusy) { yield return new WaitForSeconds(1); }
                Debug.Log("Terminated small file cleaner unit");
            }
            else if (fc.completed)
            {
                fc.completed = false;
                Toggle(GameObject.Find("IDSF"));
            }
            #endregion
            yield return new WaitForSeconds(1);
        }
    }
    private void OnApplicationQuit()
    {
        if (fc.IsBusy)
        {
            Debug.LogWarning("Application exiting, cleaning up Async thread");
            fc.terminateWorker = true;
            while (fc.IsBusy)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Debug.Log("Terminated thread");
        }
        else
            Debug.Log("No file cleaner running, terminating run time");

        if (ee.IsBusy)
        {
            Debug.LogWarning("Application exiting, cleaning up Async thread: Email extractor");
            ee.Cancel();
            while (ee.IsBusy)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Debug.Log("Terminated email extraction thread");
        }
        else
            Debug.Log("No Email extraction running, terminating run time");
    }
}
