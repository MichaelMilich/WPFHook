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
    /// <summary>
    /// The logic view model that enables all the views to show the tags of the User.
    /// The TagViewModel has an event that it publishes to the ruleviewmodel.
    /// Other than that it also containts the computer time tag that is not part of the database.
    /// It is the datacontext for all tags related views such as:
    /// addTag , DeleteTagView, TagView and ReportWindow
    /// </summary>
    public class TagViewModel : INotifyPropertyChanged
    {
        public event EventHandler TagDeleted;
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
        public string Title { get; set; }
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
        public void setEfficiencyTitle()
        {
            if(_tags.Count >1)
            {
                double eff = _tags[1].TagTime.Divide(_tags[0].TagTime) *100;
                ActivityTitle = "today effceincy =" + String.Format("{0:0.00}", eff) + "%";
            }
        }
        protected virtual void OnTagDeleted()
        {
            TagDeleted?.Invoke(this, EventArgs.Empty);
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
                SqliteDataAccess.DeleteTag(selected); // deletes from both Tags and Rules the row with this tagId
                _tags.Remove(selected);
                Tagger.UpdateTagList();
                // have to make the taggerupdate its rules in the application
                OnTagDeleted();
                // have to update the rules list in the rule view model.
                deleteTagView.Close();
            }
        }
        public void AddTag(object obj)
        {
            if (addTagView.NewTagNameTextBox.Text.Length > 0 && addTagView.NewTagColorPicker.SelectedColorText.Length > 0)
            {
                SolidColorBrush brush = new SolidColorBrush((Color)addTagView.NewTagColorPicker.SelectedColor);
                var tag = new TagModel() { TagColor = brush, TagName = addTagView.NewTagNameTextBox.Text, TagTime = new TimeSpan(0, 0, 0) };
                tag.TagID = SqliteDataAccess.saveTagAndGetId(tag);
                Tags.Add(tag);
                Tagger.UpdateTagList();
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
