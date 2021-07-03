using System;
using System.Collections.Generic;
using System.Text;

namespace WPFHook.ViewModels
{
    interface IHook
    {
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public void Start();
        public void UnHook();
    }
}
