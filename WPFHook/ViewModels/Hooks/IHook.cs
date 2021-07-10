using System;
using System.Collections.Generic;
using System.Text;
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.ViewModels
{
    interface IHook
    {
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        public void Start();
        public void UnHook();
    }
}
