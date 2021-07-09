using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using WPFHook.Models;
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
        public TimeLineWindow view;
        public TimeLineViewModel()
        {
            TimeLine first = new TimeLine();
            first.Duration = new TimeSpan(2, 0, 0);
            first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 15, 0), Duration = new TimeSpan(0, 15, 0) , TextData ="Test 1"});
            first.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 40, 0), Duration = new TimeSpan(0, 10, 0), TextData = "Test 2" });
            this.TimeLines.Add(first);

            TimeLine second = new TimeLine();
            second.Duration = new TimeSpan(2, 0, 0);
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 0, 0), Duration = new TimeSpan(0, 25, 0), TextData = "Test 3" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 30, 0), Duration = new TimeSpan(0, 15, 0), TextData = "Test 4" });
            second.Events.Add(new TimeLineEvent() { Start = new TimeSpan(0, 50, 0), Duration = new TimeSpan(0, 10, 0), TextData = "Test 5" });
            this.TimeLines.Add(second);

            view = new TimeLineWindow();
            view.TimeLineVisual.DataContext = this;
        }
        public TimeLineViewModel(List<ActivityLine> dailyList)
        {
            TimeLine day = new TimeLine();
            TimeLineEvent runningEvent = new TimeLineEvent();
            TimeSpan dayDuration = new TimeSpan();
            TimeSpan start;
            string currentTag;

            currentTag = dailyList[0].Tag;

            start = dailyList[0].StartTime;
            runningEvent.Start = dailyList[0].StartTime.Subtract(start);
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
                    runningEvent = new TimeLineEvent();
                    currentTag = dailyList[i].Tag;
                    runningEvent.Start = dailyList[i].StartTime.Subtract(start);
                    runningEvent.Duration = dailyList[i].inAppTime;
                    runningEvent.TextData = dailyList[i].ToString() + "\n";
                    runningEvent.Color = dailyList[i].TagColor;
                }
            }
            day.Events.Add(runningEvent);
            dayDuration = dailyList[dailyList.Count - 1].StartTime.Subtract(start);
            day.Duration = dayDuration;


            this.TimeLines.Add(day);

            view = new TimeLineWindow();
            view.TimeLineVisual.DataContext = this;
        }
        public TimeLineViewModel(List<ActivityLine> dailyList, ReportWindow view)
        {
            TimeLine day = new TimeLine();
            TimeLineEvent runningEvent = new TimeLineEvent();
            TimeSpan dayDuration = new TimeSpan();
            TimeSpan start;
            string currentTag;

            currentTag = dailyList[0].Tag;

            start = dailyList[0].StartTime;
            runningEvent.Start = dailyList[0].StartTime.Subtract(start);
            runningEvent.Duration = dailyList[0].inAppTime;
            runningEvent.TextData = dailyList[0].ToString() + "\n";
            runningEvent.Color = dailyList[0].TagColor;

            for (int i = 1; i < dailyList.Count; i++)
            {

                if (currentTag.Equals(dailyList[i].Tag))
                {
                    runningEvent.Duration = runningEvent.Duration.Add(dailyList[i].inAppTime);
                    runningEvent.TextData += dailyList[i].ToString() + "\n";
                }
                else
                {
                    day.Events.Add(runningEvent);
                    runningEvent = new TimeLineEvent();
                    currentTag = dailyList[i].Tag;
                    runningEvent.Start = dailyList[i].StartTime.Subtract(start);
                    runningEvent.Duration = dailyList[i].inAppTime;
                    runningEvent.TextData = dailyList[i].ToString() + "\n";
                    runningEvent.Color = dailyList[i].TagColor;
                }
            }
            day.Events.Add(runningEvent);
            dayDuration = dailyList[dailyList.Count - 1].StartTime.Subtract(start);
            day.Duration = dayDuration;


            this.TimeLines.Add(day);

            view.TimeLineVisual.DataContext = this;
        }
    }
}
