using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WPFHook.Models
{
    class TimeLine
    {
        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                 _duration = value;
            }
        }


        private ObservableCollection<TimeLineEvent> _events = new ObservableCollection<TimeLineEvent>();
        public ObservableCollection<TimeLineEvent> Events
        {
            get
            {
                return _events;
            }
            set
            {
                _events = value;
            }
        }
    }
}
