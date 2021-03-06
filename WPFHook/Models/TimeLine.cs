using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFHook.Commands;

namespace WPFHook.Models
{
    /// <summary>
    /// This is a model for a single timeline. 
    /// It is part of a code i found on the internet and i implemnted it in my project.
    /// The original code wanted to show serveral timelines in one window, i want to show only one.
    /// However, the code was good so i used it.
    /// This holds all the tImeLineEvents and has the logic for the TimeLineView usercontrol
    /// </summary>
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
        private TimeSpan _constantStart;
        private TimeSpan _constantEnd;

        public void SetConstantTimes(TimeSpan start,TimeSpan end)
        {
            _constantStart = start;
            _constantEnd = end;
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
        private Point previousPoint;
        private Point currentPoint;
        private bool isFirstMove =true;
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
        public ICommand OnReturnToConstant { get { return new RelayCommand(e => true, this.ReturnToConstant); } }
        public ICommand OnMouseMove { get { return new RelayCommand(e => true, this.MouseMove); } }
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
        public void ReturnToConstant(object obj)
        {
            this.Start = this._constantStart;
            this.End = this._constantEnd;
        }
        public void MouseMove(object obj)
        {
            if (obj is MouseEventArgs)
            {
                var e = obj as MouseEventArgs;
                var element = e.Source;
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        if (element is System.Windows.Shapes.Rectangle)
                        {
                            var rectangle = element as System.Windows.Shapes.Rectangle;
                            element = HelperStaticFunctions.FindParentOfType<System.Windows.Controls.Grid>(rectangle);
                        }
                        if (element is System.Windows.Controls.Grid)
                        {
                            var grid = element as System.Windows.Controls.Grid;
                            var delta = this.Duration.Divide(100);
                            currentPoint = e.GetPosition(grid);
                            if (isFirstMove)
                            {
                                isFirstMove = false;
                                previousPoint = currentPoint;
                            }
                            else
                            {
                                if (currentPoint.X > previousPoint.X)
                                {
                                    Start = Start.Subtract(delta);
                                    End = End.Subtract(delta);
                                }
                                else
                                {
                                    Start = Start.Add(delta);
                                    End = End.Add(delta);
                                }
                                previousPoint = currentPoint;
                            }
                        }
                    }
                }
                
            }
        }

        #endregion
    }
}
