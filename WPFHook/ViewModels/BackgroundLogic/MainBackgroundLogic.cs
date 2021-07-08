using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WPFHook.ViewModels.BackgroundLogic
{
    public class MainBackgroundLogic
    {
        public ActivityLine previousActivity;
        public SqliteDataAccess dataAccess;
        public int counter;
        public static bool isIdle = false;
        public static readonly int idleTimeInMilliseconds = 30000; // 300,000 miliseconds = 5 minutes
        public MainViewModel mainViewModel;
        public BackgroundWorker managerWindowChangedWorker;

        public MainBackgroundLogic(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            var model = mainViewModel.Model;
            dataAccess = new SqliteDataAccess();
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            model.ActivityTitle = previousActivity.ToTitle();
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
                    ActivityLine activity = e.UserState as ActivityLine;
                    mainViewModel.Model.ActivityTitle = activity.ToTitle();
                    break;
            }
        }

        private void ManagerWindowChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            counter = 0;
            isIdle = false;
            mainViewModel.currentTag = previousActivity.Tag;
            mainViewModel.Model.ActivityTitle = previousActivity.ToTitle();

        }

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

            if (isIdle)
            {
                ActivityLine activity = LoadSecondToLastActivity();
                UpdatePreviousActivity(activity);
                managerWindowChangedWorker.ReportProgress(1, activity);
            }
        }
        public void Timer_Tick(object sender, EventArgs e)
        {
            // check for idle first, then update the timers.
            var model = mainViewModel.Model;
            var timer = mainViewModel.timer;
            counter += (int)timer.Interval.TotalMilliseconds;
            if (counter > idleTimeInMilliseconds && !isIdle)
            {
                isIdle = true;
                UpdatePreviousActivity("", "Idle");
                model.ActivityTitle = previousActivity.ToTitle();
            }
            // -----NOTE FOR THE FUTURE ----
            // I should make a tag string array in the same length as the timers, maybe timers.length=tags.length+1 because i want also the global timer to be timers[0].
            // I should than do a for loop in the following manner:

            /*
            timeSpans[0] = timeSpans[0].Add(timer.Interval);
            for (int i = 1; i < timeSpans.Length;i++)
            {
                if (currentTag.Equals(tags[i-1]))
                    timeSpans[i] = timeSpans[i].Add(timer.Interval);
            }
            */

            // ----- END NOTE FOR THE FUTURE ----
            model.TotalTime = model.TotalTime.Add(timer.Interval);
            switch (mainViewModel.currentTag)
            {
                case "work":
                    model.WorkTime = model.WorkTime.Add(timer.Interval);
                    break;
                case "distraction":
                    model.DistractionTime = model.DistractionTime.Add(timer.Interval);
                    break;
                case "system":
                    model.SystemTime = model.SystemTime.Add(timer.Interval);
                    break;
            }
        }
        public void ShowDilyVisual(DateTime date)
        {
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            List<ActivityLine> dailyList = dataAccess.LoadActivities(parameter, value);
            TimeLineViewModel timeLineViewModel = new TimeLineViewModel(dailyList);
            timeLineViewModel.view.Show();
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
            dataAccess.saveActivityLine(previousActivity);
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
            dataAccess.saveActivityLine(previousActivity);
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
            dataAccess.saveActivityLine(previousActivity);
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
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = activity;
            previousActivity.SetDateAndTime(DateTime.Now);
            mainViewModel.currentTag = previousActivity.Tag;
        }

        public List<ActivityLine> LoadActivities()
        {
            return dataAccess.LoadActivities();
        }
        public ActivityLine LoadSecondToLastActivity()
        {
            return dataAccess.LoadSecondToLastActivity();
        }
        #endregion
    }
}
