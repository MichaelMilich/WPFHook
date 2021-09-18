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
using System.Windows.Media;

namespace WPFHook.ViewModels
{
    /// <summary>
    /// This class is in charge of the logics of the application.
    /// this class is the connection between the event listeners, database and so on to the GUI.
    /// IT listens to events from the event listeners, processes the data and sends the relevant information to the GUI.
    /// </summary>
    public class MainViewModel
    {
        #region constructor and background logic
        //class properties
        private MainBackgroundLogic backgroundLogic;
        public HookManager manager;

        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager, database connection, activityline object saving the last activity.
        /// </summary>
        public MainViewModel(MainWindow mainWindow)
        {
            // ------ viewmodel code---------
            view = mainWindow;
            tagViewModel = new TagViewModel(SqliteDataAccess.LoadTags());
            tagViewModel.Title = "Current App: ";
            view.TagView.DataContext = tagViewModel;
            ruleViewModel = new RuleViewModel(SqliteDataAccess.LoadRules(),tagViewModel);
            // ------ viewmodel code---------
            // ------ background logic code---------
            SetUpHook();
            backgroundLogic = new MainBackgroundLogic(this);
            currentTag = backgroundLogic.previousActivity.Tag;
            // setting the timers
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += backgroundLogic.Timer_Tick;
            // ------ background logic code---------
        }
        public void SetUpHook()
        {
            manager = new HookManager();
            manager.WindowChanged += Manager_WindowChanged;
        }

        /// <summary>
        /// Event Handler - listens to events in the hook manager.
        /// It processes the data and sends it to GUI. 
        /// It sends to window title text box the foreground process window title.
        /// To the history log it sends a log of the previous app and saves it in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            if(!backgroundLogic.managerWindowChangedWorker.IsBusy)
                backgroundLogic.managerWindowChangedWorker.RunWorkerAsync();
        }

        #endregion

        #region ViewModel region
        private MainWindow view;
        private TagViewModel tagViewModel;
        private RuleViewModel ruleViewModel;
        public RuleViewModel RuleViewModel
        {
            get { return ruleViewModel; }
        }
        public TagViewModel TagViewModel
        {
            get { return tagViewModel; }
        }
        public MainWindow View
        {
            get { return view; }
        }
        public DispatcherTimer timer;
        public string currentTag;
        #endregion


        #region ViewModelsCommands
        public ICommand ShowActivityListCommand { get { return new RelayCommand(e => true, this.ShowActivityList); } }
        public ICommand RunOnStartupCommand { get { return new RelayCommand(e => true, this.RunOnStartup); } }
        public ICommand DailyReportCommand { get { return new RelayCommand(e => true, this.DailyReport); } }
        public ICommand ShowAddTagCommand { get { return new RelayCommand(e => true, this.AddTagWindow); } }
        public ICommand ShowDeleteTagCommand { get { return new RelayCommand(e => true, this.ShowDeleteTag); } }
        public ICommand ShowAddRuleCommand { get { return new RelayCommand(e => true, this.AddRuleWindow); } }
        public ICommand ShowDeleteRuleCommand { get { return new RelayCommand(e => true, this.ShowDeleteRule); } }
        public void ShowActivityList(object obj)
        {
            ActivityDatabaseWindow subWindow = new ActivityDatabaseWindow();
            subWindow.Show();
            subWindow.ShowDataBase(SqliteDataAccess.LoadActivities());
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
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);
            TimeLineViewModel timeLineViewModel = new TimeLineViewModel(dailyList);
            DayReportViewModel reportWindowViewModel = new DayReportViewModel(getDailyReport(date), dailyList);
            reportWindowViewModel.View.TimeLineVisual.DataContext = timeLineViewModel;
            reportWindowViewModel.Show();
            
        }
        private TagViewModel getDailyReport(DateTime date)
        {
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            TagViewModel newDailyReport = new TagViewModel(SqliteDataAccess.LoadTags());
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);
            foreach (ActivityLine line in dailyList)
            {
                newDailyReport.Tags[0].TagTime = newDailyReport.Tags[0].TagTime.Add(line.inAppTime);
                for (int i=1; i<newDailyReport.Tags.Count;i++)
                {
                    if(line.Tag.Equals(newDailyReport.Tags[i].TagName))
                        newDailyReport.Tags[i].TagTime = newDailyReport.Tags[i].TagTime.Add(line.inAppTime);
                }
            }
            newDailyReport.Title = "Daily Report for " + value;
            newDailyReport.setEfficiencyTitle();
            return newDailyReport;
        }
        public void AddTagWindow(object obj)
        {
           tagViewModel.addTagView = new AddTagView();
            tagViewModel.addTagView.DataContext = tagViewModel;
            tagViewModel.addTagView.Show();
        }
        public void ShowDeleteTag(object obj)
        {
            tagViewModel.deleteTagView  = new DeleteTagView();
            tagViewModel.deleteTagView.DataContext = tagViewModel;
            tagViewModel.deleteTagView.Show();
        }
        public void AddRuleWindow(object obj)
        {
            ruleViewModel.addRuleView = new AddRuleView();
            ruleViewModel.addRuleView.DataContext = ruleViewModel;
            ruleViewModel.addRuleView.Show();
        }
        public void ShowDeleteRule(object obj)
        {
            ruleViewModel.deleteRuleWindow = new DeleteRuleWindow();
            ruleViewModel.deleteRuleWindow.DataContext = ruleViewModel;
            ruleViewModel.deleteRuleWindow.Show();
        }
        #endregion
    }
}
