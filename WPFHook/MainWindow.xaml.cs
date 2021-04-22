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
        private int counter = 0;
        //private HookManager manager;
        private MiddleMan middleMan;
        // start main window - for commits
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        /// <summary>
        /// sets up the background classes and object for the application to run.
        /// the background is : middle man+Tagger(logic), hookmanger (all events), database connection object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Tagger.StartUp();
            AddLine(counter++ + ": Initial data");
            SetUpMiddleMan();
        }
        private void SetUpMiddleMan()
        {
            middleMan = new MiddleMan();
            middleMan.UpdateHistoryLog += MiddleMan_UpdateHistoryLog;
            middleMan.UpdateWindowTitle += MiddleMan_UpdateWindowTitle;
            middleMan.ExceptionHappened += MiddleMan_ExceptionHappened;
            middleMan.AfterSettingSubscribers();
        }
        private void RemoveMiddleMan()
        {
            middleMan.appClosing();
            middleMan.UpdateHistoryLog -= MiddleMan_UpdateHistoryLog;
            middleMan.UpdateWindowTitle -= MiddleMan_UpdateWindowTitle;
            middleMan.ExceptionHappened -= MiddleMan_ExceptionHappened;
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

        private void MiddleMan_UpdateHistoryLog(object sender, string e)
        {
            AddLine(counter++ + ": " + e);
        }

        private void AddLine(string text)
        {
            historyLogBox.AppendText(text);
            historyLogBox.AppendText("\u2028"); // Linebreak, not paragraph break
            historyLogBox.ScrollToEnd();
        }
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            middleMan.appClosing();
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
        #endregion
    }
}
