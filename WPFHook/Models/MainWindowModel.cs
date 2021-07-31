using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private TagModel computerTimeTag;
        private TagModel workTimeTag;
        private TagModel distractionTimeTag;
        private TagModel systemTimeTag;

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
        public MainWindowModel(ObservableCollection<TagModel> Tags)
        {
            foreach(TagModel tagModel in Tags)
            {
                switch (tagModel.TagName)
                {
                    case "total time":
                        computerTimeTag = tagModel;
                        break;
                    case "work":
                        workTimeTag = tagModel;
                        break;
                    case "distraction":
                        distractionTimeTag = tagModel;
                        break;
                    case "system":
                        systemTimeTag = tagModel;
                        break;
                }
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
    }
}
