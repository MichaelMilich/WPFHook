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
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels
{
    /// <summary>
    /// This class is in charge of the logics of the application.
    /// this class is the connection between the event listeners, database and so on to the GUI.
    /// IT listens to events from the event listeners, processes the data and sends the relevant information to the GUI.
    /// </summary>
    public class MainViewModel
    {
        #region public
        public DispatcherTimer timer;
        public TimeSpan[] timeSpans;
        public string currentTag;
        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager, database connection, activityline object saving the last activity.
        /// </summary>
        public MainViewModel(MainWindow mainWindow)
        {
            view = mainWindow;
            backgroundLogic = new MainBackgroundLogic(this);
            currentTag = backgroundLogic.previousActivity.Tag;
            model = new MainWindowModel();
            model.ActivityTitle = "process : " + previousActivity.FGProcessName + " || window title : " + previousActivity.FGWindowName + " || " + previousActivity.Tag;
            // setting the timers
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timeSpans = new TimeSpan[4];
            for(int i =0; i<4; i++)
            {
                timeSpans[i] = new TimeSpan(); // case 0 = global time, case 1 = work time, case 2= distraction time, case 3 = system time
            }
            timer.Start();
            timer.Tick += backgroundLogic.Timer_Tick;
        }
        /// <summary>
        /// connects to the ActivityDB.db and queries the whole database
        /// </summary>
        /// <returns>List of ActivityLines that the database has (the whole database)</returns>
        public List<ActivityLine> LoadActivities()
        {
            return backgroundLogic.dataAccess.LoadActivities();
        }

        public ActivityLine LoadSecondToLastActivity()
        {
            return backgroundLogic.dataAccess.LoadSecondToLastActivity();
        }

        #endregion

        #region private / protected
        //class properties
        private MainBackgroundLogic backgroundLogic;
        private ActivityLine previousActivity;
        private SqliteDataAccess dataAccess;

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
