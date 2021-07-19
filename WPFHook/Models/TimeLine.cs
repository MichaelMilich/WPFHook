using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPFHook.Commands;

namespace WPFHook.Models
{
    class TimeLine : INotifyPropertyChanged
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
                OnPropertyChanged("Duration");
            }
        }
        private TimeSpan _start;
        public TimeSpan Start
        {
            get { return _start; }
            set 
            { 
                _start = value;
                OnPropertyChanged("Start");
                foreach(TimeLineEvent timeLineEvent in _events)
                {
                    timeLineEvent.Start = timeLineEvent.FirstStart.Subtract(_start);
                }


                _duration = _end.Subtract(_start);
                OnPropertyChanged("Duration");
            }
        }
        private TimeSpan _end;
        public TimeSpan End
        {
            get { return _end; }
            set
            {
                _end = value;
                OnPropertyChanged("End");
                _duration = _end.Subtract(_start);
                OnPropertyChanged("Duration");
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
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Commands
        public ICommand OnMouseWheelUpdateVisualDown { get { return new RelayCommand(e => true, this.MouseWheelUpdateVisualDown); } }
        public ICommand OnMouseWheelUpdateVisualUP { get { return new RelayCommand(e => true, this.MouseWheelUpdateVisualUP); } }
        public void MouseWheelUpdateVisualDown(object obj)
        {
            var delta = this.Duration.Divide(100);
            this.Start = this.Start.Subtract(delta);
            this.End = this.End.Add(delta);
        }
        public void MouseWheelUpdateVisualUP(object obj)
        {
            var delta = this.Duration.Divide(100);
            this.Start = this.Start.Add(delta);
            this.End = this.End.Subtract(delta);
        }
        #endregion
    }
}
