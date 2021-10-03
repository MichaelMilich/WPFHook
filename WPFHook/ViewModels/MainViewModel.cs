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
    /// This class is the main View Model. 
    /// It connectes between the sub- View models and the relevant Views.
    /// It also sends all the logic to a sub-class i called main backgroundLogic. The main background logic is simply there for better reading the code.
    /// </summary>
    public class MainViewModel
    {
        #region constructor and background logic
        //class properties
        private MainBackgroundLogic backgroundLogic;
        public HookManager manager;

        /// <summary>
        /// Sets up all the components of the application background such as : hooks manager,  activityline object saving the last activity.
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
        /// <summary>
        /// Sets up the Hooks of the application.
        /// The hooks are manadeg by the hook manger that sends all the information to the background logic.
        /// The hooks are :
        /// Key Hook
        /// Mouse Hook
        /// Window Hook
        /// 
        /// </summary>
        public void SetUpHook()
        {
            manager = new HookManager();
            manager.WindowChanged += Manager_WindowChanged;
        }

        /// <summary>
        /// Event Handler - listens to events in the hook manager.
        /// Sends the new messege to the backgound worker of the mainlogic.
        /// the background worker will prosess all the data and will send updates to the GUI when it is done.
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
        /// This part contains all the functions that are used by the Main View. 
        /// Some of the functions are self explanitory, so i will not comment on them.
        
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
        /// <summary>
        /// Show the Daily report.
        /// First set up a report for a certian date from the date picker.
        /// the report is shown in the report window.
        /// The report window contains a DayReport User control and a TimeLine user control.
        /// The DayReport uses a TagViewModel as the DataContext.
        /// TimeLine uses a TimeLineViewModel as dataContext.
        /// </summary>
        /// <param name="obj"></param>
        private void DailyReport(object obj)
        {
            DateTime date = (DateTime)view.dailyReportDayPicker.SelectedDate;
            if (date == null)
                date = DateTime.Now;
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);
            if (dailyList.Count == 0)
            {
                MessageBox.Show("There are no activities in " + value,"No Activities",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            else
            {
                TimeLineViewModel timeLineViewModel = new TimeLineViewModel(dailyList);
                DayReportViewModel reportWindowViewModel = new DayReportViewModel(getDailyReport(date), dailyList);
                reportWindowViewModel.View.TimeLineVisual.DataContext = timeLineViewModel;
                reportWindowViewModel.Show();
            }
            
        }
        /// <summary>
        /// Sets a Report TagViewModel with a tag list with constant timespans.
        /// The TagModel List is used to present the data as a report and will not be changed.
        /// The TagViewModel will tell the user how much time he was on the computer on a certian time and how much he was in each tag as described by the user.
        /// The efficeiny is only the firsttag devided by the computer time. not smart coding but i was tired.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
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
