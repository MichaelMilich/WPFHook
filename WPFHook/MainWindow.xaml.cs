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
        private HookManager manager;
        // start main window - for commits
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AddLine(counter++ + ": Initial data");
            manager = new HookManager();
            manager.WindowChanged += Manager_WindowChanged;
        }

        private void Manager_WindowChanged(object sender, string e)
        {
            AddLine(counter++ +": "+ e);
        }

        private void AddLine(string text)
        {
            outputBox.AppendText(text);
            outputBox.AppendText("\u2028"); // Linebreak, not paragraph break
            outputBox.ScrollToEnd();
        }
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
          // MessageBox.Show("Closing called");
            manager.UnHook();
        }
        #endregion


    }
}
