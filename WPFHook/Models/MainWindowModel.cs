using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WPFHook.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string activityTitle;
        public string ActivityTitle
        {
            get { return activityTitle; }
            set
            {
                activityTitle = value;
                OnPropertyChanged("ActivityTitle");
            }
        }
        public MainWindowModel()
        {

        }
        public MainWindowModel(string activityTitle)
        {
            this.activityTitle = activityTitle;
        }
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
