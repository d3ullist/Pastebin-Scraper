using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Extractors
{
    class Email
    {
        private const string REGEX_EMAIL = @"[A-Za-z0-9_\-\+]+@\S+([a-zA-Z]*\.?){1,10}";

        public bool IsBusy
        {
            get { return isMergeRunning; }
        }
        private bool isMergeRunning = false;

        public void Cancel()
        {
            isTerminating = true;
        }
        private bool isTerminating = false;

        public void ExtractEmailsAsync()
        {
            while (!isTerminating)
            {
                // Check if the folder exists and contains atleast 1 text file
                if (TotalFiles(Scrapers.Pastebin.SaveLocation) == 0) return;

                // Create a worker and a call back
                MergeWorkerDelegate worker = new MergeWorkerDelegate(ExtractEmailWorker);
                AsyncCallback completedCallback = new AsyncCallback(MergeCompletedCallBack);

                // Lock the operation and fire up the worker
                lock (_sync)
                {
                    if (isMergeRunning) throw new InvalidOperationException("Merge is already running, please wait till current merge completes");

                    AsyncOperation async = AsyncOperationManager.CreateOperation(null);
                    worker.BeginInvoke(completedCallback, async);
                    isMergeRunning = true;
                }
                break;
            }
        }

        //List<ExtractResult> FinalResults = new List<ExtractResult>();
        private void ExtractEmailWorker()
        {
            Regex myRegex = new Regex(REGEX_EMAIL, RegexOptions.None);
            while (!isTerminating)
            {
                foreach (string file in Files)
                {
                    // Create a result and fill in known information
                    ExtractResult ER = new ExtractResult();
                    FileInfo FI = new FileInfo(file);
                    ER.filePath = file;
                    ER.fileName = FI.Name;

                    try
                    {
                        using (StreamReader SR = new StreamReader(file))
                        {
                            string line;
                            while ((line = SR.ReadLine()) != null)
                            {
                                if (!line.Contains('@')) continue;

                                foreach (Match match in myRegex.Matches(line))
                                {
                                    ER.emails.Add(match.Value);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to read file: " + file);
                        Console.WriteLine(ex);
                    }

                    if(ER.emails.Count > 0)
                        Extract.EmailResults.Add(ER);
                }
            }
        }
        private delegate void MergeWorkerDelegate();

        private void MergeCompletedCallBack(IAsyncResult ar)
        {
            // Get the original worker delegate instance and async operation
            MergeWorkerDelegate worker = (MergeWorkerDelegate)((AsyncResult)ar).AsyncDelegate;
            AsyncOperation async = (AsyncOperation)ar.AsyncState;

            // Invoke worker end
            worker.EndInvoke(ar);

            // Clear the running task flag
            lock (_sync) { isMergeRunning = false; }

            // Raise a completion event
            AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);
            async.PostOperationCompleted(delegate(object e) { OnMergeCompleted((AsyncCompletedEventArgs)e); }, completedArgs);
        }
        public event AsyncCompletedEventHandler MergeCompleted;
        protected virtual void OnMergeCompleted(AsyncCompletedEventArgs e)
        {
            if (MergeCompleted != null)
                MergeCompleted(this, e);
            Console.WriteLine("Succes");
        }

        //static byte[] GetBytes(string str)
        //{
        //    byte[] bytes = new byte[str.Length * sizeof(char)];
        //    Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        //    return bytes;
        //}

        private string[] Files = new string[0];
        public string[] GetFiles(string folder)
        {
            if (TotalFiles(folder) == 0) return null;
            return Files;
        }
        private int TotalFiles(string folder)
        {
            if (Directory.Exists(folder))
            {
                try
                {
                    Files = Directory.GetFiles(folder, "*.txt");
                    return Files.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed: " + e);
                }
            }
            else
            {
                Console.WriteLine("Folder does not exist: " + folder);
            }
            return 0;
        }

        private readonly object _sync = new object();
    }
}
