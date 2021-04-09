using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFHook
{
    public class HookManager
    {
        private WindowHook windowHook;
        private MouseHook mouseHook;
        private Process lastProcess;
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public HookManager()
        {
            lastProcess = Process.GetCurrentProcess();
            windowHook = new WindowHook();
            windowHook.WindowChanged += Manager_WindowChanged;
            mouseHook = new MouseHook();
            mouseHook.WindowChanged += Manager_WindowChanged;
        }
        public void UnHook()
        {
            windowHook.UnHook();
            mouseHook.Stop();
        }
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            e.process = getForegroundProcess();
            if(!e.process.MainWindowTitle.Equals(lastProcess.MainWindowTitle))
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
