using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using WPFHook.Commands;
using WPFHook.Models;
using WPFHook.ViewModels.BackgroundLogic;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    class TimeLineViewModel
    {
        private ObservableCollection<TimeLine> _timeLines = new ObservableCollection<TimeLine>();
        public ObservableCollection<TimeLine> TimeLines
        {
            get
            {
                return _timeLines;
            }
            set
            {
                _timeLines= value;
            }
        }
        public TimeLineViewModel(List<ActivityLine> dailyList)
        {
            TimeLine day = new TimeLine();
            TimeSpan dayDuration = new TimeSpan();
            TimeSpan start;
            string currentTag;

            currentTag = dailyList[0].Tag;

            start = dailyList[0].StartTime;
            TimeLineEvent runningEvent = new TimeLineEvent(dailyList[0].StartTime);
            runningEvent.Start = dailyList[0].StartTime;
            runningEvent.Duration = dailyList[0].inAppTime;
            runningEvent.TextData = dailyList[0].ToString() + "\n";
            runningEvent.Color = dailyList[0].TagColor;

            for (int i=1; i<dailyList.Count;i++)
            {

                if (currentTag.Equals(dailyList[i].Tag))
                {
                    runningEvent.Duration = runningEvent.Duration.Add(dailyList[i].inAppTime);
                    runningEvent.TextData += dailyList[i].ToString() + "\n";
                }
                else
                {
                    day.Events.Add(runningEvent);
                    runningEvent = new TimeLineEvent(dailyList[i].StartTime);
                    currentTag = dailyList[i].Tag;
                    runningEvent.Start = dailyList[i].StartTime;
                    runningEvent.Duration = dailyList[i].inAppTime;
                    runningEvent.TextData = dailyList[i].ToString() + "\n";
                    runningEvent.Color = dailyList[i].TagColor;
                }
            }
            day.Events.Add(runningEvent);
            dayDuration = dailyList[dailyList.Count - 1].StartTime.Subtract(start);
            day.Duration = dayDuration;
            day.Start = start;
            day.End = dailyList[dailyList.Count - 1].StartTime;

            this.TimeLines.Add(day);
        }

    }
}
