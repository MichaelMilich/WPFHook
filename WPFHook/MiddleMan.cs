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
        public TimeSpan[] timeSpans;
        public string currentTag;
        //the events that will be triggered when other classes at this app fire an event.
        public event EventHandler<string> UpdateWindowTitle;
        public event EventHandler<Exception> ExceptionHappened;
        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager, database connection, activityline object saving the last activity.
        /// </summary>
        public MiddleMan()
        {
            counter = 0;
            manager = new HookManager();
            dataAccess = new SqliteDataAccess();
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            currentTag = previousActivity.Tag;
            manager.WindowChanged += Manager_WindowChanged;
            manager.ExceptionHappened += Manager_ExceptionHappened;
            manager.MouseMessaged += Manager_MouseMessaged;
            // setting the timers
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timeSpans = new TimeSpan[4];
            for(int i =0; i<4; i++)
            {
                timeSpans[i] = new TimeSpan(); // case 0 = global time, case 1 = work time, case 2= distraction time, case 3 = system time
            }
            timer.Start();
            timer.Tick += Timer_Tick;
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
        /// <summary>
        /// connects to the ActivityDB.db and queries the whole database
        /// </summary>
        /// <returns>List of ActivityLines that the database has (the whole database)</returns>
        public List<ActivityLine> LoadActivities()
        {
            return dataAccess.LoadActivities();
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
            manager.UnHook();
        }
        public ActivityLine LoadSecondToLastActivity()
        {
            return dataAccess.LoadSecondToLastActivity();
        }
        public (TimeSpan totalTime, TimeSpan workTime, TimeSpan distractionTime, TimeSpan systemTime) getDailyReport(DateTime date)
        {
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            TimeSpan workTime = new TimeSpan(0, 0, 0);
            TimeSpan distractionTime = new TimeSpan(0, 0, 0);
            TimeSpan systemTime = new TimeSpan(0, 0, 0);
            TimeSpan totalTime = new TimeSpan(0, 0, 0);
            List<ActivityLine> dailyList = dataAccess.LoadActivities(parameter, value);
            foreach(ActivityLine line in dailyList)
            {
                totalTime = totalTime.Add(line.inAppTime);
                switch (line.Tag)
                {
                    case "work":
                        workTime = workTime.Add(line.inAppTime);
                        break;
                    case "distraction":
                        distractionTime = distractionTime.Add(line.inAppTime);
                        break;
                    case "system":
                        systemTime = distractionTime.Add(line.inAppTime);
                        break;
                }
            }
            return (totalTime, workTime, distractionTime, systemTime);
        }
        #endregion
        #region private / protected
        //class properties
        private ActivityLine previousActivity;
        private HookManager manager;
        private SqliteDataAccess dataAccess;
        private int counter;
        private static bool isIdle = false;
        private static readonly int idleTimeInMilliseconds = 300000;
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
            UpdatePreviousActivity(e);
            // send the current window title to the app.
            string windowTitle = "process : " + e.process.ProcessName + " || window title : " + e.process.MainWindowTitle + " || " + previousActivity.Tag;
            OnUpdateWindowTitle(windowTitle);
            counter = 0;
            isIdle = false;
        }
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        private void UpdatePreviousActivity(WindowChangedEventArgs e)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, e.process.MainWindowTitle, e.process.ProcessName);
            currentTag = previousActivity.Tag;
        }
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        private void UpdatePreviousActivity(string MainWindowTitle, string ProcessName)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = new ActivityLine(DateTime.Now, MainWindowTitle, ProcessName);
            currentTag = previousActivity.Tag;
        }
        /// <summary>
        /// updates the previousActivity and the current Tag and saves the activity in the database
        /// </summary>
        /// <param name="e"></param>
        private void UpdatePreviousActivity(ActivityLine activity)
        {
            //set the previous time span
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            dataAccess.saveActivityLine(previousActivity);
            previousActivity = activity;
            previousActivity.SetDateAndTime(DateTime.Now);
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


        private void Manager_MouseMessaged(object sender, EventArgs e)
        {
            // add here code that sets the previous activity to the pre-last activity in the database
            if(isIdle)
            {
                ActivityLine activity = LoadSecondToLastActivity();
                UpdatePreviousActivity(activity);
                OnUpdateWindowTitle(activity.ToString());
            }
            counter = 0;
            isIdle = false;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // check for idle first, then update the timers.
            counter += 100;
            if(counter > idleTimeInMilliseconds && !isIdle)
            {
                isIdle = true;
                UpdatePreviousActivity("", "Idle");

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
            timeSpans[0] = timeSpans[0].Add(timer.Interval);
            switch(currentTag)
            {
                case "work":
                    timeSpans[1] = timeSpans[1].Add(timer.Interval);
                    break;
                case "distraction":
                    timeSpans[2] = timeSpans[2].Add(timer.Interval);
                    break;
                case "system":
                    timeSpans[3] = timeSpans[3].Add(timer.Interval);
                    break;
            }
        }
        #endregion
    }
}
