using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace WPFHook
{
    /// <summary>
    /// This class is in charge of the logics of the application.
    /// this class is the connection between the event listeners, database and so on to the GUI.
    /// IT listens to events from the event listeners, processes the data and sends the relevant information to the GUI.
    ///  NEED TO WORK ON EXCEPTION HANDELING
    /// </summary>
    class MiddleMan
    {
        #region public
        public DispatcherTimer timer;
        public string currentTag;
        //the events that will be triggered when other classes at this app fire an event.
        public event EventHandler<string> UpdateWindowTitle;
        public event EventHandler<Exception> ExceptionHappened;
        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager, database connection, activityline object saving the last activity.
        /// </summary>
        public MiddleMan()
        {
            manager = new HookManager();
            dataAccess = new SqliteDataAccess();
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            currentTag = previousActivity.Tag;
            manager.WindowChanged += Manager_WindowChanged;
            manager.ExceptionHappened += Manager_ExceptionHappened;
            // setting the timers
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }
        /// <summary>
        /// sets up the application to write that the current running foreground app is this one.
        /// cant be writen while setting up the object because the GUI doesnt subscribe yet to the events.
        /// </summary>
        public void AfterSettingSubscribers()
        {
            string s = "process : " + previousActivity.FGProcessName + " || window title : " + previousActivity.FGWindowName +" || "+previousActivity.Tag;
            OnUpdateWindowTitle(s);
        } 
        public List<ActivityLine> LoadActivities()
        {
            return dataAccess.LoadActivities();
        }
        /// <summary>
        /// To be used when the application is closing - to Unhook and not forget any last bits of data
        /// </summary>
        public void appClosing()
        {
            manager.UnHook();
            // maybe write the last process?
        }
        #endregion
        #region private / protected
        //class properties
        private ActivityLine previousActivity;
        private HookManager manager;
        private SqliteDataAccess dataAccess;
        
        /// <summary>
        /// escalates the exception to the main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_ExceptionHappened(object sender, Exception e)
        {
            ExceptionHappened?.Invoke(sender, e);
        }

        /// <summary>
        /// Event Handler - listens to events in the hook manager.
        /// It processes the data and sends it to GUI. 
        /// It sends to window title text box the foreground process window title.
        /// To the history log it sends a log of the previous app and saves it in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            // send the current window title to the app.
            string windowTitle = "process : " + e.process.ProcessName + " || window title : " + e.process.MainWindowTitle+" || " + previousActivity.Tag;
            OnUpdateWindowTitle(windowTitle);

            // send to history log the time of the previous app
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            windowTitle = previousActivity.ToString();
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, e.process.MainWindowTitle, e.process.ProcessName);
            currentTag = previousActivity.Tag;
        }

        /// <summary>
        /// the event that the object published to change texts
        /// </summary>
        /// <param name="windowTitle"></param>
        protected virtual void OnUpdateWindowTitle(string windowTitle)
        {

            UpdateWindowTitle?.Invoke(this, windowTitle);
        }
        #endregion
    }
}
