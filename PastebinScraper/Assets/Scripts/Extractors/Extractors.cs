//public class Extractors
//{
//    const string REGEX_EMAIL = @"[A-Za-z0-9_\-\+]+@\S+([a-zA-Z]*\.?){1,10}";
//    const string REGEX_COMBOLIST_A = @"[A-Za-z0-9_\-\+]+@\S+([a-zA-Z]*\.?){1,}:(.{6,24})";
//    const string REGEX_COMBOLIST_B = @"[A-Za-z0-9_\-\+]+@\S+([a-zA-Z]*\.?){1,};(.{6,24})";

//    public static bool E_CleanDuplicates = true;

//    public static bool E_DetectEmails = true;
//    public static bool E_DetectCombolist = true;

//    public static bool E_AutomaticExtraction = true;
//    public static string E_SaveLocation = string.Empty;

//    private static bool isBussyWorking = false;

//    private List<string> files;
//    private string CurrentFile
//    {
//        get
//        {
//            if (files == null)
//                files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Pastes").ToList();

//            string f = files[0];

//            if (f == "EmailList.txt" || f == "ComboList.txt")
//            {
//                files.RemoveAt(0);
//                f = CurrentFile;
//            }

//            files.RemoveAt(0);
//            return f;
//        }
//    }

//    public bool terminateEmailExtraction = false;
//    public bool IsBusy
//    {
//        get { return ExtractEmailIsRunning; }
//    }

//    public void ExtractEmailsAsync()
//    {
//        // Do pre liminary work here.

//        // Create a worker and a call back
//        ExtractEmailWorkerDelegate worker = new ExtractEmailWorkerDelegate(ExtractEmailWorker);
//        AsyncCallback completedCallBack = new AsyncCallback(ExtractEmailCompletedCallback);

//        // Lock the extractor, and start the task.
//        lock (_sync)
//        {
//            if (ExtractEmailIsRunning)
//                throw new InvalidOperationException("Email extraction is already running");

//            System.ComponentModel.AsyncOperation async = AsyncOperationManager.CreateOperation(null);
//            worker.BeginInvoke(completedCallBack, async);
//            ExtractEmailIsRunning = true;
//        }
//    }
//    #region ExtractEmail
//    private void ExtractEmailWorker()
//    {
//        // HACK: bypass the empty array
//        // TODO: Fix this
//        var a = CurrentFile;

//        while (files.Count > 0)
//        {
//            if (terminateEmailExtraction)
//            {
//                Debug.Log("Terminated worker early");
//                break;
//            }

//            // Assign the current file, and load it into memory
//            string title = CurrentFile;
//            string lines = File.ReadAllText(title);

//            if (!string.IsNullOrEmpty(lines))
//            {
//                Regex myRegex = new Regex(REGEX_EMAIL, RegexOptions.None);
//                HashSet<string> matches = new HashSet<string>();

//                foreach (Match match in myRegex.Matches(lines))
//                {
//                    matches.Add(match.Value);
//                }
//                Debug.Log(matches.Count + " Hits inside file " + title);

//                //TODO: Implement logic to save emails to file

//                if (matches.Count > 0 && E_DetectCombolist)
//                {
//                    //TODO: implement combo list logics
//                }
//            }

//            Thread.Sleep(1000);
//        }
//    }
//    private delegate void ExtractEmailWorkerDelegate();
//    private bool ExtractEmailIsRunning = false;
//    private void ExtractEmailCompletedCallback(IAsyncResult ar)
//    {
//        // get the original worker delegate and the AsyncOperation instance
//        ExtractEmailWorkerDelegate worker = (ExtractEmailWorkerDelegate)((AsyncResult)ar).AsyncDelegate;
//        System.ComponentModel.AsyncOperation async = (System.ComponentModel.AsyncOperation)ar.AsyncState;

//        // finish the asynchronous operation
//        worker.EndInvoke(ar);

//        // clear the running task flag
//        lock (_sync)
//        {
//            ExtractEmailIsRunning = false;
//        }

//        // raise the completed event
//        AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);
//        async.PostOperationCompleted(delegate(object e) { OnExtractEmailCompleted((AsyncCompletedEventArgs)e); }, completedArgs);

//    }
//    public event AsyncCompletedEventHandler ExtractEmailCompleted;
//    protected virtual void OnExtractEmailCompleted(AsyncCompletedEventArgs e)
//    {
//        if (ExtractEmailCompleted != null)
//            ExtractEmailCompleted(this, e);
//    }
//    private readonly object _sync = new object();
//    #endregion

//    public void ComboList(string file, string origin)
//    {
//        var myRegex = new Regex(REGEX_COMBOLIST_A, RegexOptions.None);
//        var matches = new HashSet<string>();

//        Debug.Log("Checking for combo list A");

//        foreach (Match matchedValue in myRegex.Matches(file))
//        {
//            matches.Add(matchedValue.Value);
//        }

//        if (matches.Count > 0)
//            Log.ComboList(matches, origin);
//        else
//        {
//            Debug.Log("Checking for combo list B");
//            myRegex = new Regex(REGEX_COMBOLIST_B, RegexOptions.None);

//            foreach (Match matchedValue in myRegex.Matches(file))
//            {
//                matches.Add(matchedValue.Value);
//            }

//            if (matches.Count > 0)
//                Log.ComboList(matches, origin);
//        }

//        myRegex = null;
//        matches = null;
//    }

//    private void CleanDuplicates()
//    {
//        var lines = new HashSet<string>(System.IO.File.ReadAllLines(E_SaveLocation));

//        File.WriteAllLines(E_SaveLocation, lines.ToArray());
//    }
//}
