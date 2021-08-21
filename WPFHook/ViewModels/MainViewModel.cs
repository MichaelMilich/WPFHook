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
            view.TagView.DataContext = tagViewModel;
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
        public ICommand OpenTestWindowCommand { get { return new RelayCommand(e => true, this.OpenTestWindow); } }
        public ICommand ShowAddTagCommand { get { return new RelayCommand(e => true, this.AddTagWindow); } }
        public ICommand AddTagCommand { get { return new RelayCommand(e => true, this.AddTag); } }
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
            DayReportViewModel reportWindowViewModel = new DayReportViewModel(getDailyReport(date));

            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);
            TimeLineViewModel timeLineViewModel = new TimeLineViewModel(dailyList);
            reportWindowViewModel.View.TimeLineVisual.DataContext = timeLineViewModel;
            reportWindowViewModel.Show();
        }
        private DayReportModel getDailyReport(DateTime date)
        {
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            DayReportModel dayReportModel = new DayReportModel();
            dayReportModel.Date = date;
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);
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
        private void OpenTestWindow(object obj)
        {
            Rule[] rules = new Rule[4];
            rules[0] = new Rule("FGWindowName", "Equals", "");
            rules[1] = new Rule("FGWindowName", "Contains", "ragnarok");
            rules[2] = new Rule("FGWindowName", "Contains", "facebook");
            rules[3] = new Rule("FGWindowName", "Contains", "youtube");

            DateTime date = (DateTime)view.dailyReportDayPicker.SelectedDate;
            if (date == null)
                date = DateTime.Now;
            string parameter = "Date";
            string value = date.ToString("dd/MM/yyyy");
            List<ActivityLine> dailyList = SqliteDataAccess.LoadActivities(parameter, value);

            var SystemRule =  Rule.CompileRule<ActivityLine>(rules[0]);
            var DistractionRule1= Rule.CompileRule<ActivityLine>(rules[1]);
            var DistractionRule2 = Rule.CompileRule<ActivityLine>(rules[2]);
            var DistractionRule3 = Rule.CompileRule<ActivityLine>(rules[3]);

            string[,] test = new string[2, dailyList.Count];

            for(int i=0;i< dailyList.Count;i++)
            {
                test[0, i] = dailyList[i].FGWindowName;
                if (SystemRule(dailyList[i]))
                {
                    test[1, i] = "system";
                }
                else
                {
                    if(DistractionRule1(dailyList[i]) || DistractionRule2(dailyList[i]) || DistractionRule3(dailyList[i]))
                        test[1, i] = "distraction";
                    else
                        test[1, i] = "work";
                }
            }

            TestWindow window = new TestWindow(test);
            window.Show();
            

        }
        public void AddTagWindow(object obj)
        {
           tagViewModel.addTagView = new AddTagView();
            tagViewModel.addTagView.DataContext = this;
            tagViewModel.addTagView.Show();
        }
        public void AddTag(object obj)
        {
            if (tagViewModel.addTagView.NewTagNameTextBox.Text.Length > 0 && tagViewModel.addTagView.NewTagColorPicker.SelectedColorText.Length > 0)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)tagViewModel.addTagView.NewTagColorPicker.SelectedColor);
                var tag = new TagModel() { TagColor = brush, TagName = tagViewModel.addTagView.NewTagNameTextBox.Text, TagTime = new TimeSpan(0, 20, 0) };
                tagViewModel.Tags.Add(tag);
                SqliteDataAccess.saveTag(tag);
                tagViewModel.addTagView.Close();
                
            }
            else
                MessageBox.Show("Please insert tag name and tag color", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}
