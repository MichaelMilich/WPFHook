using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
            model.ActivityTitle = "process : " + previousActivity.FGProcessName + " || window title : " + previousActivity.FGWindowName + " || " + previousActivity.Tag;
            counter = 0;
            managerWindowChangedWorker = new BackgroundWorker(); // so i want to send off a notification from my main thread (where the hooks are) to the background thread.
                                                                 // The background thread will make all the checks and database connections and will send back the results
                                                                 // The results being the properties to update and with what.
            managerWindowChangedWorker.DoWork += ManagerWindowChangedWorker_DoWork;
            managerWindowChangedWorker.RunWorkerCompleted += ManagerWindowChangedWorker_RunWorkerCompleted;
        }

        private void ManagerWindowChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // sends the UpdatePreviousActivity 
            // returns previous activity
            throw new NotImplementedException();
        }

        private void ManagerWindowChangedWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // update the tags
            throw new NotImplementedException();
        }

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


        public void Manager_ExceptionHappened(object sender, Exception e)
        {
            MessageBox.Show(e.ToString() + "\n \n " + e.Message, "Hook Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            Task LogExceptionTask = App.LogExceptions(e, e.Message);
            appClosing();
            mainViewModel.SetUpHook();
            LogExceptionTask.Wait();
        }


        public void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            e.process = getForegroundProcess();
            if (!previousActivity.FGProcessName.Equals(e.process.ProcessName) || !previousActivity.FGWindowName.Equals(e.process.MainWindowTitle))
            {
                try
                {
                    UpdatePreviousActivity(e);
                    // send the current window title to the app.
                    mainViewModel.Model.ActivityTitle = "process : " + e.process.ProcessName + " || window title : " + e.process.MainWindowTitle + " || " + previousActivity.Tag;
                    counter = 0;
                    isIdle = false;
                }
                catch (Exception ex)
                {
                    Manager_ExceptionHappened(this, ex);
                }
            }

            if (isIdle)
            {
                ActivityLine activity = mainViewModel.LoadSecondToLastActivity();
                UpdatePreviousActivity(activity);
                mainViewModel.Model.ActivityTitle = activity.ToString();
            }
            counter = 0;
            isIdle = false;
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
        public void UpdatePreviousActivity(WindowChangedEventArgs e)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, e.process.MainWindowTitle, e.process.ProcessName);
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
                model.ActivityTitle = previousActivity.ToString();
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
    }
}
