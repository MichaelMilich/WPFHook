using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
            view.DataContext = this;
        }
    }
}
