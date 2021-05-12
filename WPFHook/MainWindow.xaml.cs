using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFHook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// i am adding this part for a new commit to check myself
    /// </summary>
    public partial class MainWindow : Window
    {
        #region GUI
        //private HookManager manager;
        private MiddleMan middleMan;
        /// <summary>
        /// sets up the background classes and object for the application to run.
        /// the background is : middle man+Tagger(logic), hookmanger (all events), database connection object
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Tagger.StartUp();
            SetUpMiddleMan();
        }
        /// <summary>
        /// the function called to close the window, it uses close along with remove middle man
        /// </summary>
        public void CloseWindow()
        {
            RemoveMiddleMan();
            this.Close();
        }
        private void SetUpMiddleMan()
        {
            middleMan = new MiddleMan();
            middleMan.UpdateWindowTitle += MiddleMan_UpdateWindowTitle;
            middleMan.ExceptionHappened += MiddleMan_ExceptionHappened;
            middleMan.timer.Tick += GlobalTimer_Tick;
            middleMan.AfterSettingSubscribers();
        }
        private void RemoveMiddleMan()
        {
            middleMan.UpdateWindowTitle -= MiddleMan_UpdateWindowTitle;
            middleMan.ExceptionHappened -= MiddleMan_ExceptionHappened;
            middleMan.appClosing();
        }
        private void MiddleMan_ExceptionHappened(object sender, Exception e)
        {
            App.LogExceptions(e, e.Message);
            RemoveMiddleMan();
            SetUpMiddleMan();
        }

        private void MiddleMan_UpdateWindowTitle(object sender, string e)
        {
            currentAppBox.Text = e;
        }

        /// <summary>
        /// on asking to show the data base , call the database window - show all database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowActivityList_Click(object sender, RoutedEventArgs e)
        {
           ActivityDatabaseWindow subWindow = new ActivityDatabaseWindow();
           subWindow.Show();
           subWindow.ShowDataBase(middleMan.LoadActivities());
        }
       
        private void GlobalTimer_Tick(object sender, EventArgs e)
        {
            GlobalTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[0].Hours, middleMan.timeSpans[0].Minutes, middleMan.timeSpans[0].Seconds);
            WorkTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[1].Hours, middleMan.timeSpans[1].Minutes, middleMan.timeSpans[1].Seconds);
            DistractionTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[2].Hours, middleMan.timeSpans[2].Minutes, middleMan.timeSpans[2].Seconds);
            SystemTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[3].Hours, middleMan.timeSpans[3].Minutes, middleMan.timeSpans[3].Seconds);
        }
        #endregion

        private void LoadSecondToLastActivity_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(middleMan.LoadSecondToLastActivity().ToString());
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = (DateTime)dailyReportDayPicker.SelectedDate;
            if( date == null)
                date = DateTime.Now;
            (TimeSpan totalTime, TimeSpan workTime, TimeSpan distractionTime, TimeSpan systemTime) = middleMan.getDailyReport(date);
            ReportWindow reportWindow = new ReportWindow(date, totalTime, workTime, distractionTime, systemTime);
            reportWindow.Show();
        }
    }
}
