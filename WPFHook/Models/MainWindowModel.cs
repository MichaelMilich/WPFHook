using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string activityTitle; 
        private TagModel computerTimeTag;
        private TagModel workTimeTag;
        private TagModel distractionTimeTag;
        private TagModel systemTimeTag;
        public string ActivityTitle
        {
            get { return activityTitle; }
            set
            {
                activityTitle = value;
                OnPropertyChanged("ActivityTitle");
            }
        }
        public TagModel ComputerTimeTag
        {
            get { return computerTimeTag; }
            set
            {
                computerTimeTag = value;
                OnPropertyChanged("ComputerTimeTag");
            }
        }
        public TagModel WorkTimeTag
        {
            get { return workTimeTag; }
            set
            {
                workTimeTag = value;
                OnPropertyChanged("WorkTimeTag");
            }
        }
        public TagModel DistractionTimeTag
        {
            get { return distractionTimeTag; }
            set
            {
                distractionTimeTag = value;
                OnPropertyChanged("DistractionTimeTag");
            }
        }
        public TagModel SystemTimeTag
        {
            get { return systemTimeTag; }
            set
            {
                systemTimeTag = value;
                OnPropertyChanged("SystemTimeTag");
            }
        }

        public MainWindowModel()
        {
            computerTimeTag = new TagModel() { TagName = "total time", TagTime = new TimeSpan(0, 0, 0), TagColor = Brushes.Gray };
            workTimeTag = new TagModel() { TagName = "work time", TagTime = new TimeSpan(0, 0, 0), TagColor = Brushes.Green };
            distractionTimeTag = new TagModel() { TagName = "distraction time", TagTime = new TimeSpan(0, 0, 0), TagColor = Brushes.Red };
            systemTimeTag = new TagModel() { TagName = "system time", TagTime = new TimeSpan(0, 0, 0), TagColor = Brushes.Blue };
        }
        public MainWindowModel(string activityTitle)
        {
            this.activityTitle = activityTitle;
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
