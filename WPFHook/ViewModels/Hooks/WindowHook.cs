using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels
{
    /// <summary>
    ///  one of the hook objects. handles window changes.
    /// each time the window changes, an event is fired.
    /// basicly code i found in the internet. it does the job overall.
    ///  NEED TO WORK ON EXCEPTION HANDELING.
    /// </summary>
    public class WindowHook : IHook
    {
        #region public 
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        WinEventDelegate dele;
        IntPtr m_hhook;
        /// <summary>
        /// sets up the hook for window changes
        /// </summary>
        public WindowHook()
        {
            dele = null;
            m_hhook = IntPtr.Zero;
        }
        /// <summary>
        /// The code that is responsible to fire an event.
        /// uses SetWinEventHook to set up the hook while making WinEventProc the function to process what to do.
        /// It does so using the delegate of WinEventDelegate
        /// </summary>
        public void Start()
        {
            dele = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

        }
        public void UnHook()
        {
            UnhookWinEvent(m_hhook);
            dele = null;
        }
        /// <summary>
        /// the function that handles the processing of the eventt.
        /// in our case it sends a notification to the hook manager using another event.
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hwnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            OnWindowChanged();
        }
        /// <summary>
        /// publisher that calls for the hook manager to do the processing
        /// </summary>
        protected virtual void OnWindowChanged()
        {
            WindowChangedEventArgs args = new WindowChangedEventArgs();
            WindowChanged?.Invoke(this, args);
        }
        #endregion

        #region private
        // The constants that tell SetWinEventHook what to look for. in our case we will be using EVENT_SYSTEM_FOREGROUND.
        // should make it into a struct.
        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;
        private const int EVENT_SYSTEM_FOREGROUND = 3;


        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);
        #endregion
    }
}
