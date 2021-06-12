using System;
using System.Threading.Tasks;
using System.Windows;
using WPFHook.Models;
using WPFHook.ViewModels;

namespace WPFHook.Views
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
            middleMan = new MiddleMan(this);
            middleMan.ExceptionHappened += MiddleMan_ExceptionHappened;
            middleMan.timer.Tick += GlobalTimer_Tick;
            DataContext = middleMan;
        }
        private void RemoveMiddleMan()
        {
            middleMan.ExceptionHappened -= MiddleMan_ExceptionHappened;
            middleMan.appClosing();
        }
        private void MiddleMan_ExceptionHappened(object sender, Exception e)
        {
            MessageBox.Show(e.ToString() + "\n \n " + e.Message, "Hook Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            Task LogExceptionTask = App.LogExceptions(e, e.Message);
            RemoveMiddleMan();
            SetUpMiddleMan();
            LogExceptionTask.Wait();
        }
       
        private void GlobalTimer_Tick(object sender, EventArgs e)
        {
            GlobalTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[0].Hours, middleMan.timeSpans[0].Minutes, middleMan.timeSpans[0].Seconds);
            WorkTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[1].Hours, middleMan.timeSpans[1].Minutes, middleMan.timeSpans[1].Seconds);
            DistractionTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[2].Hours, middleMan.timeSpans[2].Minutes, middleMan.timeSpans[2].Seconds);
            SystemTimerDisplay.Text = string.Format("{0}:{1}:{2}", middleMan.timeSpans[3].Hours, middleMan.timeSpans[3].Minutes, middleMan.timeSpans[3].Seconds);
        }
        #endregion
    }
}
