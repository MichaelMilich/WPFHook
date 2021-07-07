using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace WPFHook.ViewModels
{
    public class ActivityLine
    {
        private DateTime dateAndTime;
        public DateTime DateAndTime => dateAndTime;

        public int id;
        public string Date => dateAndTime.ToString("dd/MM/yyyy");
        public string Time => dateAndTime.ToString("HH:mm:ss");
        public string FGWindowName { get; set; }
        public string FGProcessName { get; set; }
        public TimeSpan inAppTime { get; set; }
        public string InAppTime => string.Format("{0}:{1}:{2}", inAppTime.Hours, inAppTime.Minutes, inAppTime.Seconds);
        public string Tag { get; private set; }
        public void updateTag()
        {
            Tag = Tagger.getTag(FGWindowName, FGProcessName);
        }
        public ActivityLine(Int64 id, string Date, string Time, string FGWindowName, string FGProcessName, string InAppTime, string Tag)
        {
            this.id = (int)id;
            this.dateAndTime = ParseDateAndTime(Date, Time);
            this.FGWindowName = FGWindowName;
            this.FGProcessName = FGProcessName;
            this.inAppTime = ParseTimeSpan(InAppTime);
            this.Tag = Tag;
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
        public DateTime ParseDateAndTime(string Date, string Time)
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            return DateTime.ParseExact(Date + " " + Time, "dd/MM/yyyy HH:mm:ss", culture);
        }
        public void SetDateAndTime(DateTime dateAndTime)
        {
            this.dateAndTime = dateAndTime;
        }
        public TimeSpan ParseTimeSpan(string time) => TimeSpan.Parse(time);

        public override string ToString()
        {
            string s = Date + " || " + Time + " || ";
            s += FGProcessName + " || ";
            s += FGWindowName + " || ";
            s += Tag + " || ";
            s += InAppTime;
            return s;
        }

    }
}
