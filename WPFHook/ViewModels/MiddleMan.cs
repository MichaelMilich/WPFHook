using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WPFHook.Commands;
using WPFHook.Models;
using WPFHook.ViewModels;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    /// <summary>
    /// This class is in charge of the logics of the application.
    /// this class is the connection between the event listeners, database and so on to the GUI.
    /// IT listens to events from the event listeners, processes the data and sends the relevant information to the GUI.
    /// </summary>
    public class MiddleMan
    {
        #region public
        public DispatcherTimer timer;
        public TimeSpan[] timeSpans;
        public string currentTag;
        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager, database connection, activityline object saving the last activity.
        /// </summary>
        public MiddleMan(MainWindow mainWindow)
        {
            view = mainWindow;
            counter = 0;
            managerBackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            SetUpHook();
            dataAccess = new SqliteDataAccess();
            previousActivity = new ActivityLine(Process.GetCurrentProcess().StartTime, Process.GetCurrentProcess().MainWindowTitle, Process.GetCurrentProcess().ProcessName);
            currentTag = previousActivity.Tag;
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
            model = new MainWindowModel();
            model.ActivityTitle = "process : " + previousActivity.FGProcessName + " || window title : " + previousActivity.FGWindowName + " || " + previousActivity.Tag;
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries the whole database
        /// </summary>
        /// <returns>List of ActivityLines that the database has (the whole database)</returns>
        public List<ActivityLine> LoadActivities()
        {
            return dataAccess.LoadActivities();
        }

        public ActivityLine LoadSecondToLastActivity()
        {
            return dataAccess.LoadSecondToLastActivity();
        }

        #endregion

        #region private / protected
        //class properties
        private ActivityLine previousActivity;
        private HookManager manager;
        private BackgroundWorker managerBackgroundWorker;
        private SqliteDataAccess dataAccess;
        private int counter;
        private static bool isIdle = false;
        private static readonly int idleTimeInMilliseconds = 300000; // 300,000 miliseconds = 5 minutes
        #region Hooks

        private void SetUpHook()
        {
            manager = new HookManager(managerBackgroundWorker);
            managerBackgroundWorker.DoWork += manager.BackgroundWorkerOnDoWork;
            managerBackgroundWorker.ProgressChanged += ManagerBackgroundWorker_ProgressChanged;
            managerBackgroundWorker.RunWorkerAsync();
        }

        private void ManagerBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //MessageBox.Show("progress" + e.UserState.ToString(), "messeged!", MessageBoxButton.OK, MessageBoxImage.Information);
            var userObject = e.UserState;
            if (userObject is string)
            {
                string func = userObject as string;
                if(func.Equals("mouse messeged"))
                {
                    Manager_MouseMessaged(sender, e);
                }
                if (func.Equals("window changed"))
                {
                    WindowChangedEventArgs args = new WindowChangedEventArgs();
                    Manager_WindowChanged(sender, args);
                }
            }

        }

        /// <summary>
        /// escalates the exception to the main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_ExceptionHappened(object sender, Exception e)
        {
            MessageBox.Show(e.ToString() + "\n \n " + e.Message, "Hook Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            Task LogExceptionTask = App.LogExceptions(e, e.Message);
            appClosing();
            SetUpHook();
            LogExceptionTask.Wait();
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
            e.process = getForegroundProcess();
            if (!previousActivity.FGProcessName.Equals(e.process.ProcessName) || !previousActivity.FGWindowName.Equals(e.process.MainWindowTitle))
            {
                try
                {
                    UpdatePreviousActivity(e);
                    // send the current window title to the app.
                    model.ActivityTitle = "process : " + e.process.ProcessName + " || window title : " + e.process.MainWindowTitle + " || " + previousActivity.Tag;
                    counter = 0;
                    isIdle = false;
                }
                catch (Exception ex)
                {
                    Manager_ExceptionHappened(this, ex);
                }
            }
        }
        /// <summary>
        /// code i found in the internet
        /// returns the foreground process by using processID.
        /// need to read more about it and have edge cases delt with.
        /// </summary>
        /// <returns></returns>
        private Process getForegroundProcess()
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
        #endregion
        #region UpdatePreviousActivity
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
        #endregion
        /// <summary>
        /// To be used when the application is closing - to Unhook and not forget any last bits of data
        /// </summary>
        private void appClosing()
        {
            //save the current activity and than close the app.
            previousActivity.inAppTime = DateTime.Now.Subtract(previousActivity.DateAndTime);
            dataAccess.saveActivityLine(previousActivity);
            //all that is left is to close the app
            managerBackgroundWorker.CancelAsync();
            manager.UnHook();
        }
        private void Manager_MouseMessaged(object sender, EventArgs e)
        {
            // add here code that sets the previous activity to the pre-last activity in the database
            if(isIdle)
            {
                ActivityLine activity = LoadSecondToLastActivity();
                UpdatePreviousActivity(activity);
                model.ActivityTitle = activity.ToString();
            }
            counter = 0;
            isIdle = false;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // check for idle first, then update the timers.
            counter += (int)timer.Interval.TotalMilliseconds;
            if(counter > idleTimeInMilliseconds && !isIdle)
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
            model.TotalTime= model.TotalTime.Add(timer.Interval);
            switch (currentTag)
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
        #endregion

        #region ViewModel region
        private MainWindow view;
        private MainWindowModel model;
        public MainWindowModel Model
        {
            get { return model; }
        }
        #endregion


        #region ViewModelsCommands
        public ICommand ShowActivityListCommand { get { return new RelayCommand(e => true, this.ShowActivityList); } }
        public ICommand LoadSecondToLastActivityCommand { get { return new RelayCommand(e => true, this.LoadSecondToLastActivity); } }
        public ICommand RunOnStartupCommand { get { return new RelayCommand(e => true, this.RunOnStartup); } }
        public ICommand DailyReportCommand { get { return new RelayCommand(e => true, this.DailyReport); } }
        public void ShowActivityList(object obj)
        {
            ActivityDatabaseWindow subWindow = new ActivityDatabaseWindow();
            subWindow.Show();
            subWindow.ShowDataBase(LoadActivities());
        }
        private void LoadSecondToLastActivity(object obj)
        {
            MessageBox.Show(this.LoadSecondToLastActivity().ToString());
        }
        private void RunOnStartup(object obj)
        {
            MessageBoxResult result = MessageBox.Show("Do you want the DailyLog to run on startup?", "DailyLog", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                App.SetStartup(true);
            else if (result == MessageBoxResult.No)
                App.SetStartup(false);
        }
        private void DailyReport(object obj)
        {
            DateTime date = (DateTime)view.dailyReportDayPicker.SelectedDate;
            if (date == null)
                date = DateTime.Now;
            DayReportViewModel reportWindowViewModel = new DayReportViewModel(getDailyReport(date));
            reportWindowViewModel.Show();
        }
        private DayReportModel getDailyReport(DateTime date)
        {
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            DayReportModel dayReportModel = new DayReportModel();
            dayReportModel.Date = date;
            List<ActivityLine> dailyList = dataAccess.LoadActivities(parameter, value);
            foreach (ActivityLine line in dailyList)
            {
                dayReportModel.TotalTime = dayReportModel.TotalTime.Add(line.inAppTime);
                switch (line.Tag)
                {
                    case "work":
                        dayReportModel.WorkTime = dayReportModel.WorkTime.Add(line.inAppTime);
                        break;
                    case "distraction":
                        dayReportModel.DistractionTime = dayReportModel.DistractionTime.Add(line.inAppTime);
                        break;
                    case "system":
                        dayReportModel.SystemTime = dayReportModel.SystemTime.Add(line.inAppTime);
                        break;
                }
            }
            dayReportModel.Data = dailyList;
            return dayReportModel;
        }
        #endregion
    }
}
