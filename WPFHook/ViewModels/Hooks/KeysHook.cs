using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels.Hooks
{
    class KeysHook : IHook
    {
        #region public
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public KeysHook()
        {
            _proc = HookCallback;
            _hookID = IntPtr.Zero;
        }
        public void Start() => _hookID = SetHook(_proc);
        public void UnHook() => UnhookWindowsHookEx(_hookID);
        #endregion
        #region private
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID;

        private delegate IntPtr LowLevelKeyboardProc( int nCode, IntPtr wParam, IntPtr lParam);

        private  IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private  IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            OnWindowChanged();
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected virtual void OnWindowChanged()
        {
            WindowChangedEventArgs args = new WindowChangedEventArgs();
            WindowChanged?.Invoke(this, args);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

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
