using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using UnityEngine;

public class FileCleaner
{
    public bool terminateWorker = false;
    public bool IsBusy
    {
        get { return removeUnderIsRunning; }
    }

    public bool completed; 

    public void RemoveUnderAsync(int charAmount, string folder)
    {
        if (TotalFiles(folder) == 0)
        {
            Debug.LogWarning("No files found inside directory: " + folder);
            return;
        }

        RemoveUnderWorkerDelegate worker = new RemoveUnderWorkerDelegate(RemoveUnderWorker);
        AsyncCallback completedCallback = new AsyncCallback(RemoveUnderCompletedCallback);

        lock (_sync)
        {
            if (removeUnderIsRunning)
                throw new InvalidOperationException("Remove Under is already running");

            System.ComponentModel.AsyncOperation async = AsyncOperationManager.CreateOperation(null);
            worker.BeginInvoke(charAmount, completedCallback, async);
            removeUnderIsRunning = true;
        }
    }
    #region RemoveUnder
    private bool removeUnderIsRunning = false;
    private void RemoveUnderWorker(int charAmount)
    {
        // Assuming encoding takes up 20 bytes, and a char exists of 2 bytes.
        int byteLimit = 20 + charAmount * 2;
        Debug.Log(FileLocations.Length);
        foreach (string file in FileLocations)
        {
            if (terminateWorker)
            {
                Debug.Log("Terminated worker early");
                break;
            }

            FileInfo fi = new FileInfo(file);
            long size = fi.Length;

            if (size <= byteLimit)
            {
                Debug.Log("File: " + file + " Contained less then " + byteLimit + " Bytes Marked for deletion");

                try { File.Delete(fi.FullName); Main.stats.SmallFilesDeleted++; }
                catch(Exception e) {Debug.Log("Failed to delete small file!!!!! " + e);}

                continue;
            }
            Debug.Log("CLEARED");
            Thread.Sleep(1);
        }
        completed = true;
    }
    private delegate void RemoveUnderWorkerDelegate(int charAmount);
    private void RemoveUnderCompletedCallback(IAsyncResult ar)
    {
        // get the original worker delegate and the AsyncOperation instance
        RemoveUnderWorkerDelegate worker = (RemoveUnderWorkerDelegate)((AsyncResult)ar).AsyncDelegate;
        System.ComponentModel.AsyncOperation async = (System.ComponentModel.AsyncOperation)ar.AsyncState;

        // finish the asynchronous operation
        worker.EndInvoke(ar);

        // clear the running task flag
        lock (_sync)
        {
            removeUnderIsRunning = false;
        }

        // raise the completed event
        AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);
        async.PostOperationCompleted(delegate(object e) { OnRemoveUnderCompleted((AsyncCompletedEventArgs)e); }, completedArgs);
    }
    public event AsyncCompletedEventHandler RemoveUnderCompleted;
    protected virtual void OnRemoveUnderCompleted(AsyncCompletedEventArgs e)
    {
        if (RemoveUnderCompleted != null)
            RemoveUnderCompleted(this, e);
    }
    private readonly object _sync = new object();
    #endregion

    private string[] FileLocations = new string[0];
    private int TotalFiles(string folder)
    {
        if (Directory.Exists(folder))
        {
            FileLocations = Directory.GetFiles(folder, "*.txt");
            return FileLocations.Length;
        }
        else
        {
            Debug.LogWarning("Directory did not exist: " + folder);
            return 0;
        }
    }

    private void OnApplicationQuit()
    {
        if (IsBusy)
        {
            Debug.LogWarning("Application exiting, cleaning up Async filecleaner thread");
            terminateWorker = true;
            while (IsBusy)
            {
                Thread.Sleep(1000);
            }

            Debug.Log("Terminated filecleaner thread");
        }
    }
}
