using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    /// <summary>
    /// Model of a timeline Event.
    /// This is a new object and not an ActtivityLine because i wanted to save instances of activityLines on the GUI.
    /// Basicly an TimeLineEvent hold all the data of subsequent ActivityLines from the same Tag.
    /// On a normal day it makes the amount of blocks to show in report are lowered by 50%.
    /// </summary>
    class TimeLineEvent : INotifyPropertyChanged
    {
        private TimeSpan firstStart;
        public TimeSpan FirstStart { get { return firstStart; } }
        private TimeSpan start;
        public TimeSpan Start
        {
            get { return start; }
            set
            { 
                start = value;
                OnPropertyChanged("Start");
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged("Duration");
            }
        }    
        private string textData;
        public string TextData
        {
            get { return textData; }
            set { textData = value; }
        }
        private Brush color;
        public Brush Color
        {
            get { return color; }
            set { color = value; }
        }
        public TimeLineEvent(TimeSpan Duration, TimeSpan Start)
        {
            duration = Duration;
            start = Start;
        }

        public TimeLineEvent()
        {

        }
        public TimeLineEvent(TimeSpan firstStart)
        {
            this.firstStart = firstStart;
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
