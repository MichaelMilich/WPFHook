using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFHook
{
    public class HookManager
    {
        #region public 
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        WinEventDelegate dele = null;
        IntPtr m_hhook = IntPtr.Zero;
        public HookManager()
        {
            SetHook();
        }
        public void UnHook()
        {
            UnhookWinEvent(m_hhook);
            dele = null;
        }
        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            string windowTitle ="";
            Process foregroundProcess = getForegroundProcess();
            windowTitle = DateTime.Now.ToString("HH:mm:ss") + " || ";
            windowTitle += foregroundProcess.MainWindowTitle + " || ";
            windowTitle += "tags (to be implemented)";
            OnWindowChanged(foregroundProcess);
        }
        protected virtual void OnWindowChanged(Process foregroundProcess)
        {
            WindowChangedEventArgs args = new WindowChangedEventArgs();
            args.process = foregroundProcess;
            WindowChanged?.Invoke(this, args);
        }
        #endregion

        #region private
        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;
        private const int EVENT_SYSTEM_FOREGROUND = 3;
        private void SetHook()
        {
            dele = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
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
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);
        #endregion
    }
}
