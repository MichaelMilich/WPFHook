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
using WPFHook.Models;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    public class TagViewModel : INotifyPropertyChanged
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
        private string activityTitle;
        public string ActivityTitle
        {
            get { return activityTitle; }
            set
            {
                activityTitle = value;
                OnPropertyChanged("ActivityTitle");
            }
        }
        public AddTagView addTagView;
        public TagViewModel()
        {

        }
        public TagViewModel(List<TagModel> tags)
        {
            foreach(TagModel model in tags)
            {
                _tags.Add(model);
            }
        }
        public TagViewModel(MainWindowModel mainWindowModel)
        {
           // activityTitle = mainWindowModel.ActivityTitle;
            _tags.Add(mainWindowModel.ComputerTimeTag);
            _tags.Add(mainWindowModel.WorkTimeTag);
            _tags.Add(mainWindowModel.DistractionTimeTag);
            _tags.Add(mainWindowModel.SystemTimeTag);
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
