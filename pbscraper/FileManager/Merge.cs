using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace FileManager
{
    public class Merger
    {
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

        public string destinationFile;
        public string destinationPath;

        public void MergeFilesAsync(int byteSize, string location)
        {
            while (!isTerminating)
            {
                // Check if the folder exists and contains atleast 1 text file
                if (TotalFiles(location) == 0) return;

                // Generate a file named after the first object in files
                try
                {
                    string path = string.Format(@"{0}\Merge{1}bytes", location, byteSize);

                    for (int i = 0; i < 1000; i++)
                    {
                        if (i == 0)
                        {
                            if (File.Exists(path + ".txt")) continue;
                            else
                            {
                                path = path + ".txt";
                                FileStream f = File.Create(path);
                                f.Close();
                                break;
                            }
                        }

                        if (File.Exists(path + i + ".txt")) continue;
                        else
                        {
                            path = path + i + ".txt";
                            FileStream f = File.Create(path);
                            f.Close();
                            break;
                        }
                    }
                    destinationPath = location;
                    destinationFile = path;
                }
                catch (Exception e) { Console.WriteLine("Failed to create a merge file with reason: " + e); return; }

                // Create a worker and a call back
                MergeWorkerDelegate worker = new MergeWorkerDelegate(MergeWorker);
                AsyncCallback completedCallback = new AsyncCallback(MergeCompletedCallBack);

                // Lock the operation and fire up the worker
                lock (_sync)
                {
                    if (isMergeRunning) throw new InvalidOperationException("Merge is already running, please wait till current merge completes");

                    AsyncOperation async = AsyncOperationManager.CreateOperation(null);
                    worker.BeginInvoke(byteSize, completedCallback, async);
                    isMergeRunning = true;
                }
                break;
            }
        }

        private void MergeWorker(int byteSize)
        {
            long maxByteSize = 5368709120; // 1024*1024*1024*5 (5gb)
            long maxFiles = maxByteSize / byteSize; // Rough estimate of how many files of max size would make 5gb
            long altMaxFiles = toMerge(ref Files, byteSize).Count; // In case there are less available use that amount instead
            
            if (maxFiles > altMaxFiles)
                maxFiles = altMaxFiles;

            long counter = 0;

            using (Stream destinationStream = File.OpenWrite(destinationFile))
            {
                foreach (string file in toMerge(ref Files, byteSize))
                {
                    if (isTerminating) return;
                    if (counter++ >= maxFiles) break;
                    try
                    {
                        using (Stream sourceStream = File.OpenRead(file))
                        {
                            // Create a byte array for a blank line
                            byte[] blankLine = GetBytes(Environment.NewLine + Environment.NewLine);

                            // Copy the file to our destination
                            sourceStream.CopyTo(destinationStream);

                            // Add a blank line to the file
                            int bytesRead = 0;
                            while (bytesRead < blankLine.Length - 1)
                            {
                                destinationStream.WriteByte(blankLine[bytesRead]);
                                bytesRead++;
                            }
                        }
                    }
                    catch { continue; }
                }
            }

            while (toMerge(ref Files, byteSize).Count > 0)
            {
                foreach (string file in toMerge(ref Files, byteSize))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        Console.WriteLine("FAIL: " + file);
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        private delegate void MergeWorkerDelegate(int byteSize);

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

        List<string> toMerge(ref string[] Files, int byteSize)
        {
            List<string> holder = new List<string>();

            foreach (string file in GetFiles(destinationPath))
            {
                if (isTerminating) return null;

                // Create info of the file to determine size
                FileInfo fi = new FileInfo(file);
                long size = fi.Length;

                if (size <= byteSize)
                {
                    holder.Add(file);
                }
            }
            return holder;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

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
