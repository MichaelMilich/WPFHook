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

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Tagger.StartUp();
            AddLine(counter++ + ": Initial data");
            middleMan = new MiddleMan();
            middleMan.UpdateHistoryLog += MiddleMan_UpdateHistoryLog;
            middleMan.UpdateWindowTitle += MiddleMan_UpdateWindowTitle;
            middleMan.AfterSettingSubscribers();
            //manager = new HookManager();
            //manager.WindowChanged += Manager_WindowChanged;
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
            // MessageBox.Show("Closing called");
            // manager.UnHook();
            middleMan.appClosing();
        }
        private void ShowActivityList_Click(object sender, RoutedEventArgs e)
        {
           ActivityDatabaseWindow subWindow = new ActivityDatabaseWindow();
           subWindow.Show();
           subWindow.ShowDataBase(middleMan.LoadActivities());
        }
        #endregion
    }
}
