using System.Collections;
using System.IO;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace Extractors
{
    public class Email
    {
        private const string REGEX_EMAIL = @"[A-Za-z0-9_\-\+]+@\S+([a-zA-Z]*\.?){1,10}";

        private string FilesLocation;
        private string SaveLocation;
        private bool CleanDuplicates;
        private bool terminateRuntime;

        private string[] filePaths;

        public Email() { }
        public Email(string saveLocation, bool cleanDuplicates)
        {
            if (!VerifyLocation(saveLocation, true)) throw new DirectoryNotFoundException("Could not find the SaveLocation, try another one");

            this.SaveLocation = saveLocation;
            this.CleanDuplicates = cleanDuplicates;
        }
        public Email(string filesLocation, string saveLocation, bool cleanDuplicates)
        {
            if (!VerifyLocation(filesLocation, false)) throw new DirectoryNotFoundException("Could not find the FileLocations, try another one");
            if (!VerifyLocation(saveLocation, true)) throw new DirectoryNotFoundException("Could not find the SaveLocation, try another one");

            this.FilesLocation = filesLocation;
            this.SaveLocation = saveLocation;
            this.CleanDuplicates = cleanDuplicates;
        }

        public void ExtractAsync()
        {
            terminateRuntime = false;

            if (filePaths == null)
                filePaths = Directory.GetFiles(FilesLocation, "*.txt", SearchOption.TopDirectoryOnly);

            if (filePaths.Length <= 0)
                throw new NullReferenceException("No files found at path");

            ExtractWorkerDelegate worker = new ExtractWorkerDelegate(ExtractWorker);
            AsyncCallback completedCallback = new AsyncCallback(ExtractCompletedCallback);

            lock (_sync)
            {
                if (extractIsRunning) throw new InvalidOperationException("Email Extraction is already running, create a new extractor, or terminate the current one first");

                AsyncOperation async = AsyncOperationManager.CreateOperation(null);
                worker.BeginInvoke(completedCallback, async);
                extractIsRunning = true;
            }
        }
        public void ExtractAsync(string saveLocation, bool cleanDuplicates)
        {
            if (!VerifyLocation(saveLocation, true)) throw new DirectoryNotFoundException("Could not find the SaveLocation, try another one");

            this.SaveLocation = saveLocation;
            this.CleanDuplicates = cleanDuplicates;
            ExtractAsync();
        }
        public void ExtractAsync(string filesLocation, string saveLocation, bool cleanDuplicates)
        {
            if (!VerifyLocation(filesLocation, false)) throw new DirectoryNotFoundException("Could not find the FileLocations, try another one");
            if (!VerifyLocation(saveLocation, true)) throw new DirectoryNotFoundException("Could not find the SaveLocation, try another one");

            this.FilesLocation = filesLocation;
            this.SaveLocation = saveLocation;
            this.CleanDuplicates = cleanDuplicates;

            ExtractAsync();
        }
        #region Extractor
        private readonly object _sync = new object();
        private bool extractIsRunning = false;

        public event AsyncCompletedEventHandler ExtractCompleted;

        private void ExtractWorker()
        {
            Regex myRegex = new Regex(REGEX_EMAIL, RegexOptions.None);

            for (int i = 0; i < filePaths.Length; i++)
            {
                if (terminateRuntime)
                {
                    UnityEngine.Debug.Log("Terminated early at " + i + " of " + filePaths.Length);
                    break;
                }

                string[] lines = File.ReadAllLines(filePaths[i]);

                if (lines.Length > 0)
                {
                    HashSet<string> matches = new HashSet<string>();

                    for (int e = 0; e < lines.Length; e++)
                    {
                        foreach (Match match in myRegex.Matches(lines[e]))
                        {
                            matches.Add(match.Value);
                        }
                    }
                    UnityEngine.Debug.Log("Going to sleep");
                    Thread.Sleep(1000);
                    UnityEngine.Debug.Log("Awake again");
                }
                UnityEngine.Debug.Log("Going to sleep in between files");
                Thread.Sleep(1000);
                UnityEngine.Debug.Log("Awake again for the next file");
            }
            UnityEngine.Debug.Log("Going to sleep at the end");
            Thread.Sleep(1000);
            UnityEngine.Debug.Log("Awake again before ending");
        }
        private delegate void ExtractWorkerDelegate();

        private void ExtractCompletedCallback(IAsyncResult ar)
        {
            // get the original worker delegate and the AsyncOperation instance
            ExtractWorkerDelegate worker = (ExtractWorkerDelegate)((AsyncResult)ar).AsyncDelegate;
            System.ComponentModel.AsyncOperation async = (System.ComponentModel.AsyncOperation)ar.AsyncState;

            // finish the asynchronous operation
            worker.EndInvoke(ar);

            // clear the running task flag
            lock (_sync)
            {
                extractIsRunning = false;
            }

            // raise the completed event
            AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);
            async.PostOperationCompleted(delegate(object e) { OnExtractCompleted((AsyncCompletedEventArgs)e); }, completedArgs);
        }

        protected virtual void OnExtractCompleted(AsyncCompletedEventArgs e)
        {
            if (ExtractCompleted != null)
                ExtractCompleted(this, e);
        }
        #endregion

        public void Cancel()
        {
            terminateRuntime = true;
        }
        public bool IsTerminating
        {
            get { return terminateRuntime; }
        }
        public bool IsBusy
        {
            get { return extractIsRunning; }
        }

        private bool VerifyLocation(string location, bool createIfNonExists)
        {
            if (string.IsNullOrEmpty(location))
            {
                UnityEngine.Debug.LogError("Trying to verify location with a empty location");
                return false;
            }
            if (Directory.Exists(location)) return true;
            else
            {
                UnityEngine.Debug.LogWarning("Folder does not exist, Attempting to create folder");
                if (createIfNonExists)
                {
                    if (CreateFolder(location))
                    {
                        UnityEngine.Debug.Log("Succesfully created folder at: " + location);
                        return true;
                    }
                    else throw new ArgumentNullException("No folder could be found or created at: " + location + " Try another location instead");
                }
                else throw new ArgumentNullException("No folder could be found at: " + location + " Try another location instead");
            }
        }
        private bool CreateFolder(string location)
        {
            try { Directory.CreateDirectory(location); return true; }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Could not create folder: " + location + " || " + e);
                return false;
            }
        }
    }
}