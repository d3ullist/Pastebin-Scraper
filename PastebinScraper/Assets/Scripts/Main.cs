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
    private Statistics stats = new Statistics();
    private FileCleaner fc = new FileCleaner();

	IEnumerator Start () 
    {
        Debug.Log("Main Loop running");
        while (Application.isPlaying)
        {
            if (IsScrapingPastebin) pastebin.Scrape();

            if (!fc.IsBusy && IsDeletingSmallFiles)
            {
                if (fc.terminateWorker) fc.terminateWorker = false;

                Debug.Log("Attempt to start deleting of small files");
                fc.RemoveUnderAsync(AmountOfChars, pastebin.SaveLocation);
            }
            else if (fc.IsBusy && !IsDeletingSmallFiles)
            {
                Debug.Log("Reported running of deleting small files, terminating service");
                fc.terminateWorker = true;
            }

            if (!ee.IsBusy && IsExtractingEmails)
            {
                Debug.Log("Attempt to start extraction of emails");
                ee.ExtractAsync(pastebin.SaveLocation, pastebin.SaveLocation, true);
            }
            else if (ee.IsBusy && !IsExtractingEmails)
            {
                Debug.Log("Reported running of email extractor, terminating now.");
                ee.Cancel();
                Debug.Log("Terminated email extractor");
            }

            // Setting next estimate scrape time, and wait for 5 minutes
            int index = DateTime.Now.AddMinutes(5).TimeOfDay.ToString().IndexOf(".");
            if (index > 0)
                nextScrape = DateTime.Now.AddMinutes(5).TimeOfDay.ToString().Substring(0, index);
            yield return new WaitForSeconds(300);
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
