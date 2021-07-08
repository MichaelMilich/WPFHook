using System;
using System.Collections.Generic;
using System.Text;

namespace WPFHook.Models
{
    class TimeLineEvent
    {
        private TimeSpan start;
        public TimeSpan Start
        {
            get { return start; }
            set { start = value; }
        }
        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; }
        }
        public TimeLineEvent(TimeSpan Duration, TimeSpan Start)
        {
            duration = Duration;
            start = Start;
        }
        private string textData;
        public string TextData
        {
            get { return textData; }
            set { textData = value; }
        }
        public TimeLineEvent()
        {

        }
    }
}
