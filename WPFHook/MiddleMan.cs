using System;
using System.Collections.Generic;
using System.Text;

namespace WPFHook
{
    class MiddleMan
    {
        private HookManager manager;
        public event EventHandler<string> UpdateHistoryLog;
        public event EventHandler<string> UpdateWindowTitle;
        public MiddleMan()
        {
            manager = new HookManager();
            manager.WindowChanged += Manager_WindowChanged;
        }
        /// <summary>
        /// Event Handler - listens to events in the hook manager.
        /// It processes the data and sends it to GUI. 
        /// It sends to window title text box the foreground process window title.
        /// To the history log it sends a log of the previous app - to be implemented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            // send the current window title to the app.
            string windowTitle = e.process.MainWindowTitle;
            OnUpdateWindowTitle(windowTitle);

            // send to history log the time of the previous app
            // ---------- TO BE IMPLEMENTED-------------------
            windowTitle = "";
            windowTitle = DateTime.Now.ToString("HH:mm:ss") + " || ";
            windowTitle += e.process.MainWindowTitle + " || ";
            windowTitle += "tags (to be implemented)";
            OnUpdateHistoryLog(windowTitle);
        }

        public void appClosing()
        {
            manager.UnHook();
        }
        protected virtual void OnUpdateHistoryLog(string windowTitle)
        {

            UpdateHistoryLog?.Invoke(this, windowTitle);
        }
        protected virtual void OnUpdateWindowTitle(string windowTitle)
        {

            UpdateWindowTitle?.Invoke(this, windowTitle);
        }
    }
}
