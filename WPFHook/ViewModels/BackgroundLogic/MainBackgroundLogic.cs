using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFHook.Models;
using WPFHook.Views;
using System.Windows.Media;

namespace WPFHook.ViewModels.BackgroundLogic
{
    /// <summary>
    /// The Logic Part of the MainViewModel.
    /// An object of this class goes hand to hand with the mainVeiwModel and has a pointer to its instance.
    /// This class handles writing and saving to the Activity.db
    /// There are some bugs that i should deal with in this class in next revisions.
    /// For instance, i would like to scrap the idle function or at least change it.
    /// </summary>
    public class MainBackgroundLogic
    {
        public ActivityLine previousActivity; // the current activity that is currently runing on the foreground, waiting to be written into the database.
        public int counter; // counter that will check if the yser became idle.
        public int dayCounter; // every hour it will check the date, if there is a new date than update the timer and dates.
        public DateTime currentDate;
        public static bool isIdle = false;
        public static readonly int idleTimeInSeconds = 300; // 300 seconds = 5 minutes
        public MainViewModel mainViewModel;
        public BackgroundWorker managerWindowChangedWorker;

        /// <summary>
        /// Set Up the MainBackground Logic.
        /// The hard part to understand from my point of view is the BackgroundWorker.
        /// </summary>
        /// <param name="mainViewModel"></param>
        public MainBackgroundLogic(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            var tagModel = mainViewModel.TagViewModel;
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            tagModel.ActivityTitle = previousActivity.ToTitle();
            currentDate = DateTime.Today;
            dayCounter = 0;
            counter = 0;
            managerWindowChangedWorker = new BackgroundWorker(); // so i want to send off a notification from my main thread (where the hooks are) to the background thread.
                                                                 // The background thread will make all the checks and database connections and will send back the results
                                                                 // The results being the properties to update and with what.
            managerWindowChangedWorker.WorkerReportsProgress = true;
            managerWindowChangedWorker.DoWork += ManagerWindowChangedWorker_DoWork;
            managerWindowChangedWorker.RunWorkerCompleted += ManagerWindowChangedWorker_RunWorkerCompleted;
            managerWindowChangedWorker.ProgressChanged += ManagerWindowChangedWorker_ProgressChanged;
        }
        #region logic main functions
        private void ManagerWindowChangedWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0: // case 0 - there is an error - if so, deal with it.
                    Exception ex = e.UserState as Exception;
                    MessageBox.Show(ex.ToString() + "\n \n " + ex.Message, "Hook Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Task LogExceptionTask = App.LogExceptions(ex, ex.Message);
                    appClosing();
                    mainViewModel.SetUpHook();
                    LogExceptionTask.Wait();
                    break;
                case 1: // case 1 - the computer came of being idle.
                    isIdle = false;
                    ActivityLine activity = e.UserState as ActivityLine;
                    mainViewModel.TagViewModel.ActivityTitle = activity.ToTitle();
                    break;
                case 2: // case 2 - there was a hook messege but no change in database.
                    isIdle = false;
                    break;
            }
        }
        /// <summary>
        /// This event is called each time the background finnished a call.
        /// At the end of processing the ActivityLine it has to set the counter to 0, set the idle bool to false and update the tag as well as the title of the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManagerWindowChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            counter = 0;
            isIdle = false;
            mainViewModel.currentTag = previousActivity.Tag;
            mainViewModel.TagViewModel.ActivityTitle = previousActivity.ToTitle();

        }
        /// <summary>
        /// The hard wor that is done in a seperate thread by the background worker.
        /// The hard lifting as I see it is the getForegroundProcess(); that is called from the [DllImport("user32.dll")].
        /// After that the application saves the activity into the database.
        /// This has to be in the background as to not hold up the Hooks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManagerWindowChangedWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var process = getForegroundProcess();
            if (!previousActivity.FGProcessName.Equals(process.ProcessName) || !previousActivity.FGWindowName.Equals(process.MainWindowTitle))
            {
                try
                {
                    UpdatePreviousActivity(process);
                }
                catch (Exception ex)
                {
                    managerWindowChangedWorker.ReportProgress(0, ex); // the number will be code for what situation it is 0 =exception
                }
            }
            else if (isIdle) // As i written before, i  might want to change this code (see comment in the Timer_Tick - idle section 19/09/21 21:09)
            {
                ActivityLine activity = SqliteDataAccess.LoadSecondToLastActivity();
                UpdatePreviousActivity(activity);
                managerWindowChangedWorker.ReportProgress(1, activity);
            }
        }
        public void Timer_Tick(object sender, EventArgs e)
        {
            // check for idle first, then update the timers.
            var tagViewModel = mainViewModel.TagViewModel;
            var timer = mainViewModel.timer;
            counter += (int)timer.Interval.TotalSeconds; // adds time in integer to check if the computer is idle.
            dayCounter+= (int)timer.Interval.TotalSeconds;
            if(dayCounter> 3600) // if it has been more than an hour (3600 seconds)
            {
                if(!currentDate.Equals(DateTime.Today))
                {
                    currentDate = DateTime.Today;
                    mainViewModel.View.dailyReportDayPicker.SelectedDate = DateTime.Today;
                    foreach (TagModel tagModel in tagViewModel.Tags)
                    {
                        tagModel.TagTime = new TimeSpan(0, 0, 0);
                    }
                }
                dayCounter = 0;
            }
            // this part checks if the computer became idle. 
            // Currently it will save the activity before the computer became idle and will start a new activity loging how much time the user was idle.
            // I think to change it so that it will not log anything untill the user gets out of Idle. Just a thought. Michael Millich 19/09/2021 21:09.
            if (counter > idleTimeInSeconds && !isIdle) 
            {
                isIdle = true;
                UpdatePreviousActivity("", "Idle");
                tagViewModel.ActivityTitle = previousActivity.ToTitle();
            }

            tagViewModel.Tags[0].TagTime = tagViewModel.Tags[0].TagTime.Add(timer.Interval);
            foreach(TagModel tagModel in tagViewModel.Tags)
            {
                if (tagModel.TagName.Equals(mainViewModel.currentTag))
                    tagModel.TagTime = tagModel.TagTime.Add(timer.Interval);
            }
        }
        #endregion
        #region helping functions
        /// <summary>
        /// To be used when the application is closing - to Unhook and not forget any last bits of data
        /// </summary>
        public void appClosing()
        {
            //save the current activity and than close the app.
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            SqliteDataAccess.saveActivityLine(previousActivity);
            //all that is left is to close the app
            mainViewModel.manager.WindowChanged -= mainViewModel.Manager_WindowChanged;
            mainViewModel.manager.UnHook();
        }

        /// <summary>
        /// code i found in the internet
        /// returns the foreground process by using processID.
        /// need to read more about it and have edge cases delt with.
        /// </summary>
        /// <returns></returns>
        public Process getForegroundProcess()
        {
            uint processID = 0;
            IntPtr handle = IntPtr.Zero;
            handle = GetForegroundWindow();
            uint threadID = GetWindowThreadProcessId(handle, out processID); // Get PID from window handle
            Process foregroundProcess = Process.GetProcessById(Convert.ToInt32(processID)); // Get it as a C# obj.
            // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
            return foregroundProcess;
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        public void UpdatePreviousActivity(Process e)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            SqliteDataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, e.MainWindowTitle, e.ProcessName);
            mainViewModel.currentTag = previousActivity.Tag;
        }
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        public void UpdatePreviousActivity(string MainWindowTitle, string ProcessName)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            SqliteDataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, MainWindowTitle, ProcessName);
            mainViewModel.currentTag = previousActivity.Tag;
        }
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        public void UpdatePreviousActivity(ActivityLine activity)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            SqliteDataAccess.saveActivityLine(previousActivity);
            previousActivity = activity;
            previousActivity.SetDateAndTime(DateTime.Now);
            mainViewModel.currentTag = previousActivity.Tag;
        }
        /// <summary>
        /// Obselete Code, Might completle delete it in another revision.
        /// </summary>
        public static void CheckFirstTime()
        {
            /*
            var tagList = SqliteDataAccess.LoadTags();
            if (tagList.Count==0)
            {
                TagModel tag = new TagModel() { TagName = "System", TagColor = Brushes.Blue };
                SqliteDataAccess.saveTag(tag);
                tag = new TagModel() { TagName = "Distraction", TagColor = Brushes.Red };
                SqliteDataAccess.saveTag(tag);
                tag = new TagModel() { TagName = "Work", TagColor = Brushes.Green };
                SqliteDataAccess.saveTag(tag);
            }
            var ruleList = SqliteDataAccess.LoadRules();
            if( ruleList.Count==0)
            {
                RuleModel rule = new RuleModel("FGWindowName", "Equals", "",1);
                SqliteDataAccess.saveRule(rule);
                rule = new RuleModel("FGWindowName", "Contains", "youtube",2);
                SqliteDataAccess.saveRule(rule);
                rule = new RuleModel("FGWindowName", "Contains", "facebook",2);
                SqliteDataAccess.saveRule(rule);
                rule = new RuleModel("FGWindowName", "Contains", "ragnarok",2);
                SqliteDataAccess.saveRule(rule);
                rule = new RuleModel(RuleModel.everythingElseRuleString, RuleModel.everythingElseRuleString, RuleModel.everythingElseRuleString, 3);
                SqliteDataAccess.saveRule(rule);
            }
            */
        }
        #endregion
    }
}
