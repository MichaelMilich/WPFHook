using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using WPFHook.Models;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    class TagViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TagModel> _tags = new ObservableCollection<TagModel>();
        public ObservableCollection<TagModel> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
            }
        }
        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get { return _totalTime; }
            set
            {
                _totalTime = value;
                OnPropertyChanged("TotalTime");
            }
        }
        /*
        private TagView _view;
        public TagView View
        {
            get { return _view; }
        }*/
        public TagViewModel()
        {
            _totalTime = new TimeSpan(1, 0, 0);
            _tags.Add(new TagModel() { TagColor = Brushes.Blue, TagName = "test1", TagTime = new TimeSpan(0, 15, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Red, TagName = "test2", TagTime = new TimeSpan(0, 20, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Green, TagName = "My name Is Misha", TagTime = new TimeSpan(0, 5, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Violet, TagName = "Miki", TagTime = new TimeSpan(0, 20, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Purple, TagName = "sdfsdf", TagTime = new TimeSpan(0, 20, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Tan, TagName = "sdfsdf", TagTime = new TimeSpan(0, 23, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Firebrick, TagName = "fghrrnr", TagTime = new TimeSpan(0, 45, 0) });
            TestWindow window = new TestWindow();
            window.Report.DataContext = this;
            window.Show();

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
    }
}
