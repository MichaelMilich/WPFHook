using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.ViewModels.BackgroundLogic
{
    /// <summary>
    /// The Basic Building Block of the application.
    /// All the application functions have to use the activityline.
    /// This currently has the data of the Window Name, the Process name, the starttime and date of start time.
    /// This class heavly relies on the Tagger Class to ay what tag it belongs to.
    /// 
    /// I can always add more objects to save in the activityline.
    /// I will have to update the Activity.db database and the SQLiteconnection class.
    /// But other than that, its easy.
    /// </summary>
    public class ActivityLine
    {
        private DateTime dateAndTime;
        public DateTime DateAndTime => dateAndTime;

        public int id;
        public string Date => dateAndTime.ToString("dd/MM/yyyy");
        public string Time => dateAndTime.ToString("HH:mm:ss");
        public TimeSpan StartTime => dateAndTime.TimeOfDay;
        public string FGWindowName { get; set; }
        public string FGProcessName { get; set; }
        public TimeSpan inAppTime { get; set; }
        public string InAppTime => string.Format("{0}:{1}:{2}", inAppTime.Hours, inAppTime.Minutes, inAppTime.Seconds);
        public string Tag { get; private set; }
        public Brush TagColor;
        public void updateTag()
        {
            (Tag, TagColor) = Tagger.getTag(this);
        }
        public ActivityLine(Int64 id, string Date, string Time, string FGWindowName, string FGProcessName, string InAppTime, string Tag)
        {
            this.id = (int)id;
            this.dateAndTime = ParseDateAndTime(Date, Time);
            this.FGWindowName = FGWindowName;
            this.FGProcessName = FGProcessName;
            this.inAppTime = ParseTimeSpan(InAppTime);
            this.Tag = Tag;
            TagColor = Tagger.UpdateTagColor(Tag);
        }
        public ActivityLine(DateTime dateAndTime, string windowName,string processName, string tag)
        {
            this.dateAndTime = dateAndTime;
            this.FGWindowName = windowName;
            this.FGProcessName = processName;
            this.Tag = tag;
            TagColor = Tagger.UpdateTagColor(Tag);
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
        public string ToTitle()
        {
            string s = "Window: " + FGWindowName + " || Tag: " + Tag;
            return s;
        }

    }
}
