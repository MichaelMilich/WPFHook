using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFHook
{
    /// <summary>
    /// This class is the gateway to all hooks and event listeners of the app outside the app.
    /// an object of this class will have all the objects that handle the hooks.
    /// NEED TO WORK ON EXCEPTION HANDELING
    /// test 
    /// </summary>
    public class HookManager
    {
        // system event hooks that are in place.
        // it might be a good idea to have a general hook object or interface that all hooks works with.
        private WindowHook windowHook;
        private MouseHook mouseHook;
        private Process lastProcess;
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public event EventHandler<Exception> ExceptionHappened;
        /// <summary>
        /// sets up all the hooks for the windows events.
        /// </summary>
        public HookManager()
        {
            lastProcess = Process.GetCurrentProcess();
            windowHook = new WindowHook();
            windowHook.WindowChanged += Manager_WindowChanged;
            mouseHook = new MouseHook();
            mouseHook.WindowChanged += Manager_WindowChanged;
        }
        /// <summary>
        /// use this when closing the app
        /// </summary>
        public void UnHook()
        {
            windowHook.UnHook();
            mouseHook.Stop();
        }
        /// <summary>
        /// the general subscriber for all windows events.
        /// each objects publishes an event with the same delegate. they all sbscribed here.
        /// if the window title or the process have changed - than invoke the rest of the events in the middle man and so on.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            try 
            {
                e.process = getForegroundProcess();
                if (!e.process.MainWindowTitle.Equals(lastProcess.MainWindowTitle))
                {
                    lastProcess = e.process;
                    WindowChanged?.Invoke(this, e);
                }
                else
                {
                    if (!e.process.ProcessName.Equals(lastProcess.ProcessName))
                    {
                        lastProcess = e.process;
                        WindowChanged?.Invoke(this, e);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHappened?.Invoke(this, ex);
            }
        }
        /// <summary>
        /// code i found in the internet
        /// returns the foreground process by using processID.
        /// need to read more about it and have edge cases delt with.
        /// </summary>
        /// <returns></returns>
        private Process getForegroundProcess()
        {
            uint processID = 0;
            IntPtr handle = IntPtr.Zero;
            handle = GetForegroundWindow();
            uint threadID = GetWindowThreadProcessId(handle, out processID); // Get PID from window handle
            Process foregroundProcess = Process.GetProcessById(Convert.ToInt32(processID)); // Get it as a C# obj.
            // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
            return foregroundProcess;
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}
