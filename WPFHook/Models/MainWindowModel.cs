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
        private TimeSpan totalTime;
        private TimeSpan workTime;
        private TimeSpan distractionTime;
        private TimeSpan systemTime;
        public TimeSpan TotalTime
        {
            get { return totalTime; }
            set
            {
                totalTime = value;
                OnPropertyChanged("TotalTime");
            }
        }
        public TimeSpan WorkTime
        {
            get { return workTime; }
            set
            {
                workTime = value;
                OnPropertyChanged("WorkTime");
            }
        }
        public TimeSpan DistractionTime
        {
            get { return distractionTime; }
            set
            {
                distractionTime = value;
                OnPropertyChanged("DistractionTime");
            }
        }
        public TimeSpan SystemTime
        {
            get { return systemTime; }
            set
            {
                systemTime = value;
                OnPropertyChanged("SystemTime");
            }
        }
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
