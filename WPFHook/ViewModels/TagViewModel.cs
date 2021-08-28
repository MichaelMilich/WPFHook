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
using WPFHook.ViewModels.BackgroundLogic;
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
        public DeleteTagView deleteTagView;
        public TagViewModel()
        {

        }
        public TagViewModel(List<TagModel> tags)
        {
            _tags.Add(new TagModel() {TagName="Computer Time", TagColor=Brushes.Gray,TagTime=new TimeSpan(0,0,0) }); // adding the General Time Tag
            foreach(TagModel model in tags)
            {
                _tags.Add(model);
            }
        }

        #region ViewModel commands
        public ICommand AddTagCommand { get { return new RelayCommand(e => true, this.AddTag); } }
        public ICommand DeleteTagComand { get { return new RelayCommand(e => true, this.DeleteTag); } }
        public void DeleteTag(object obj)
        {
            if (deleteTagView.tagsComboBox.SelectedItem.Equals(Tags[0]))
            {
                MessageBox.Show("Can't delete the Computer time!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (MessageBox.Show("Are You sure you want to delete this tag?","Question",MessageBoxButton.YesNoCancel,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                TagModel selected = ((TagModel)deleteTagView.tagsComboBox.SelectedItem);
                SqliteDataAccess.DeleteTag(selected);
                _tags.Remove(selected);
                deleteTagView.Close();
            }
        }
        public void AddTag(object obj)
        {
            if (addTagView.NewTagNameTextBox.Text.Length > 0 && addTagView.NewTagColorPicker.SelectedColorText.Length > 0)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)addTagView.NewTagColorPicker.SelectedColor);
                var tag = new TagModel() { TagColor = brush, TagName = addTagView.NewTagNameTextBox.Text, TagTime = new TimeSpan(0, 20, 0) };
                tag.TagID = SqliteDataAccess.saveTagAndGetId(tag);
                Tags.Add(tag);
                addTagView.Close();

            }
            else
                MessageBox.Show("Please insert tag name and tag color", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
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
