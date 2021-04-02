using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WPFHook
{
    public class WindowChangedEventArgs : EventArgs
    {
        public Process process { get; set; }
    }
}
