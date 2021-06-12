using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WPFHook.ViewModels
{
    public class WindowChangedEventArgs : EventArgs
    {
        public Process process { get; set; }
    }
}
