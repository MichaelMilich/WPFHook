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
        private TimeSpan _start;
        public TimeSpan Start
        {
            get { return _start; }
            set { _start = value; }
        }
        private TimeSpan _end;
        public TimeSpan End
        {
            get { return _end; }
            set { _end = value; }
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
