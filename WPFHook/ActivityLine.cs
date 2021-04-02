using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WPFHook
{
    class ActivityLine
    {
        private DateTime dateAndTime;
        public DateTime DateAndTime => dateAndTime;
        public string Date => dateAndTime.ToString("dd/MM/yyyy");
        public string Time => dateAndTime.ToString("HH:mm:ss");
        public string FGWindowName { get; set; }
        public string FGProcessName { get; set; }
        public TimeSpan inAppTime { get; set; }
        public string InAppTimeString => string.Format("{0}:{1}:{2}", inAppTime.Hours, inAppTime.Minutes, inAppTime.Seconds);
        public string Tag { get; private set; }
        public void updateTag()
        {
            if (FGWindowName.Equals("chrome"))
                Tag = "distraction";
            else
                Tag = "work";
        }
        public ActivityLine(DateTime dateAndTime, string windowName,string processName, string tag)
        {
            this.dateAndTime = dateAndTime;
            this.FGWindowName = windowName;
            this.FGProcessName = processName;
            this.Tag = tag;
        }
        public ActivityLine(DateTime dateAndTime, string windowName, string processName)
        {
            this.dateAndTime = dateAndTime;
            this.FGWindowName = windowName;
            this.FGProcessName = processName;
            updateTag();
        }
        public ActivityLine(DateTime dateAndTime)
        {
            this.dateAndTime = dateAndTime;
        }

        public DateTime ParseDateAndTime()
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            return DateTime.ParseExact(this.Date + " " + this.Time, "dd/MM/yyyy HH:mm:ss", culture);
        }
        public TimeSpan ParseTimeSpan(string time) => TimeSpan.Parse(time);

        public override string ToString()
        {
            string s = Date + " || " + Time + " || ";
            s += FGProcessName + " || ";
            s += FGWindowName + " || ";
            s += Tag + " || ";
            s += InAppTimeString;
            return s;
        }
    }
}
