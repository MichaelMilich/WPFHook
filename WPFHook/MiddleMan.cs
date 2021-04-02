using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WPFHook
{
    class MiddleMan
    {
        private ActivityLine previousActivity;
        private HookManager manager;
        private SqliteDataAccess dataAccess;
        public event EventHandler<string> UpdateHistoryLog;
        public event EventHandler<string> UpdateWindowTitle;
        public MiddleMan()
        {
            manager = new HookManager();
            dataAccess = new SqliteDataAccess();
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            manager.WindowChanged += Manager_WindowChanged;
        }
        public void AfterSettingSubscribers()
        {
            string s = "process : " + previousActivity.FGProcessName + " || window title : " + previousActivity.FGWindowName;
            OnUpdateWindowTitle(s);
        }
        /// <summary>
        /// Event Handler - listens to events in the hook manager.
        /// It processes the data and sends it to GUI. 
        /// It sends to window title text box the foreground process window title.
        /// To the history log it sends a log of the previous app 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            // send the current window title to the app.
            string windowTitle = "process : " + e.process.ProcessName + " || window title : " + e.process.MainWindowTitle;
            OnUpdateWindowTitle(windowTitle);

            // send to history log the time of the previous app
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            windowTitle = previousActivity.ToString();
            OnUpdateHistoryLog(windowTitle);
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, e.process.MainWindowTitle, e.process.ProcessName);
        }
        public void appClosing()
        {
            manager.UnHook();
        }
        protected virtual void OnUpdateHistoryLog(string windowTitle)
        {

            UpdateHistoryLog?.Invoke(this, windowTitle);
        }
        protected virtual void OnUpdateWindowTitle(string windowTitle)
        {

            UpdateWindowTitle?.Invoke(this, windowTitle);
        }
    }
}
