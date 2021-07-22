using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WPFHook.ViewModels.BackgroundLogic;
using WPFHook.ViewModels.Hooks;

namespace WPFHook.ViewModels
{
    /// <summary>
    /// This class is the gateway to all hooks and event listeners of the app outside the app.
    /// an object of this class will have all the objects that handle the hooks.
    /// NEED TO WORK ON EXCEPTION HANDELING
    /// test 
    /// </summary>
    public class HookManager: IHook
    {
        #region public
        // system event hooks that are in place.
        // it might be a good idea to have a general hook object or interface that all hooks works with.
        public event EventHandler<WindowChangedEventArgs> WindowChanged;
        /// <summary>
        /// sets up all the hooks for the windows events.
        /// </summary>
        public HookManager()
        {
            hooks = new List<IHook>();
            SetHooks();
            Subscribe();
            this.Start();
        }


        /// <summary>
        /// use this when closing the app
        /// </summary>
        public void UnHook()
        {
            foreach(IHook hook in hooks)
            {
                hook.UnHook();
            }
        }
        public void Start()
        {
            foreach (IHook hook in hooks)
            {
                hook.Start();
            }
        }
        #endregion
        #region private
        private List<IHook> hooks;
        private void SetHooks()
        {
            hooks.Add(new MouseHook() as IHook);
            hooks.Add(new WindowHook() as IHook);
            hooks.Add(new KeysHook() as IHook);
        }
        private void Subscribe()
        {
            foreach (IHook hook in hooks)
            {
                hook.WindowChanged += Manager_WindowChanged;
            }
        }
        /// <summary>
        /// the general subscriber for all windows events.
        /// each objects publishes an event with the same delegate. they all sbscribed here.
        /// if the window title or the process have changed - than invoke the rest of the events in the middle man and so on.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_WindowChanged(object sender, WindowChangedEventArgs e)
        {
            WindowChanged?.Invoke(sender, e);
        }

        #endregion
    }
}
