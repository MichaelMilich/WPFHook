using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WPFHook.ViewModels.BackgroundLogic
{
    public class WindowChangedEventArgs : EventArgs
    {
        public Process process { get; set; }
    }
}
