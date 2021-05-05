using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFHook
{
    /// <summary>
    /// one of the hook objects. handles mouse moves.
    /// each time the mouse moves, an event is fired.
    /// basicly code i found in the internet. it does the job overall.
    ///  NEED TO WORK ON EXCEPTION HANDELING.
    /// </summary>
    public class MouseHook
    {
        #region public 
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public event EventHandler MouseMessaged;
        /// <summary>
        /// sets up the hook for mouse moves
        /// </summary>
        public MouseHook()
        {
            _proc = HookCallback;
            _hookID = IntPtr.Zero;
            Start();
        } 
        public void Start() => _hookID = SetHook(_proc);
        public  void Stop() => UnhookWindowsHookEx(_hookID);

        #endregion

        #region private 
        private LowLevelMouseProc _proc;
        private IntPtr _hookID;
        /// <summary>
        /// Sets up the hook for mouse moves.
        /// usses SetWindowsHookEx and sets the handling funtion to be HookCallback by the delegate LowLevelMouseProc proc.
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                  GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// The code that is responsible to fire an event.
        /// SetWindowsHookEx sets the hook up, HookCallback tells what to do once there was a the mouse moved
        /// at the end of the function MUST USE CallNextHookEx
        /// </summary>
        /// <param name="nCode">int that indicates if the messege should be processed by this function. if <0 than should be processed</param>
        /// <param name="wParam">idetifier and pointer of the mouse messege</param>
        /// <param name="lParam"> mouse messege structure</param>
        /// <returns></returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(nCode >= 0)
                MouseMessaged?.Invoke(this, EventArgs.Empty);
            if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                OnWindowChanged();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        /// <summary>
        /// a constant used for the SetWindowsHookEx to indicate the function to
        /// Installs a hook procedure that monitors low-level mouse input events.
        /// </summary>
        private const int WH_MOUSE_LL = 14;

        /// <summary>
        /// struct of the mouse messeges.
        /// example: if you want to see if the mouse moved, check if the messege is WM_MOUSEMOVE
        /// </summary>
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        /// <summary>
        /// publishing event to the hook manager. 
        /// should make it an interface since it is also used in the hook manager and window change hook.
        /// </summary>
        protected virtual void OnWindowChanged()
        {
            WindowChangedEventArgs args = new WindowChangedEventArgs();
            WindowChanged?.Invoke(this, args);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
