using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WPFHook.ViewModels;
using WPFHook.Views;

namespace WPFHook
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// sets up the exceptions and icon of the application.
    /// NOTE - NO STARTUP URI IN THE XAML FILE, THE STARTUP OFF MAIN WINDOW IS IN on_Stratup.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;
        private MiddleMan mainViewModel;
        public App() : base()
        {
            SetupUnhandledExceptionHandling();
        }
        #region Exceptions
        private static int counter = 1;
        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };

        }

        private void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            Task LogExceptionTask = LogExceptions(e, unhandledExceptionType);

            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }
            LogExceptionTask.Wait();
            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        public static async Task LogExceptions(Exception e, string unhandledExceptionType)
        {
            string s = DateTime.Now.ToString() + " Exception #" + counter + " :" + $"Unexpected Error Occurred: {unhandledExceptionType} " + $"The following exception occurred:\n{e}\n";
            using StreamWriter file = new StreamWriter("ExceptionLog.txt", append: true);
            await file.WriteLineAsync(s);
        }
        #endregion
        #region background run

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool isExit;
        /// <summary>
        /// What to do with the application on startup
        /// open a new MainWindow (NOTE, THE XMAL FILE DOESNT HAVE STARTUP URI).
        /// set the notification icon.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetUpView();
            SetUpViewModel();
            Tagger.StartUp();
        }
        private void SetUpView()
        {
            mainWindow = new MainWindow();
            mainWindow.Closing += MainWindow_Closing;
            mainWindow.ExitHeader.Click += ExitHeader_Click;
            isExit = false;
            CreateNotificationIcon();
        }
        private void SetUpViewModel()
        {
            mainViewModel = new MiddleMan(mainWindow);
            mainWindow.DataContext = mainViewModel;
        }
        private void ExitHeader_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        /// <summary>
        /// creates the icon with all the notifications.
        /// </summary>
        private void CreateNotificationIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon(); //sets the icon
            notifyIcon.DoubleClick += (s, args) => ShowMainWindow(); // sets when double clicked, use ShowMainWindow()

            // seting up the icon
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //setting up the context.
            notifyIcon.ContextMenuStrip =new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Open").Click += (s, e) => ShowMainWindow();
            notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
            
            //setting up the icon hover text and baloontip text
            notifyIcon.Text = "M.DailyLog";
            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            notifyIcon.BalloonTipTitle = "M.DailyLog still runs in the background";
            notifyIcon.BalloonTipText = "To exit, use notification bar";
            
            //making the notification icon visible, end of function.
            notifyIcon.Visible = true;
        }
        /// <summary>
        /// what to do on shutdown
        /// </summary>
        private void ExitApplication()
        {
            isExit = true;
            mainWindow.Close();
            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (mainWindow.IsVisible)
            {
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
            }
            else
            {
                mainWindow.Show();
            }
        }
        /// <summary>
        /// listens to closing event of the main window,
        /// only closes if the exit is called from the icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!isExit)
            {
                notifyIcon.ShowBalloonTip(3000);
                e.Cancel = true;
                mainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
        public static void SetStartup(bool isOnStartUp)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if(isOnStartUp)
            {
                rk.SetValue("WPFHook", AppDomain.CurrentDomain.BaseDirectory +"WPFHook.exe");
            }
            else
            {
                rk.DeleteValue("WPFHook", false);
            }
        }
        #endregion
    }
}
