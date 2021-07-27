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
        /*
        private TagView _view;
        public TagView View
        {
            get { return _view; }
        }*/
        private AddTagView addTagView;
        public TagViewModel()
        {
           // _totalTime = new TimeSpan(1, 0, 0);
            _tags.Add(new TagModel() { TagColor = Brushes.Blue, TagName = "test1", TagTime = new TimeSpan(0, 15, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Red, TagName = "test2", TagTime = new TimeSpan(0, 20, 0) });
            //_tags.Add(new TagModel() { TagColor = Brushes.Green, TagName = "My name Is Misha", TagTime = new TimeSpan(0, 5, 0) });
            _tags.Add(new TagModel() { TagColor = Brushes.Violet, TagName = "Miki", TagTime = new TimeSpan(0, 20, 0) });
            //_tags.Add(new TagModel() { TagColor = Brushes.Purple, TagName = "sdfsdf", TagTime = new TimeSpan(0, 20, 0) });
            //_tags.Add(new TagModel() { TagColor = Brushes.Tan, TagName = "sdfsdf", TagTime = new TimeSpan(0, 23, 0) });
            //_tags.Add(new TagModel() { TagColor = Brushes.Firebrick, TagName = "fghrrnr", TagTime = new TimeSpan(0, 45, 0) });
            TestWindow window = new TestWindow();
            window.Report.DataContext = this;
            window.DataContext = this;
            window.Show();

        }
        public TagViewModel(MainWindowModel mainWindowModel)
        {
            activityTitle = mainWindowModel.ActivityTitle;
            _tags.Add(mainWindowModel.ComputerTimeTag);
            _tags.Add(mainWindowModel.WorkTimeTag);
            _tags.Add(mainWindowModel.DistractionTimeTag);
            _tags.Add(mainWindowModel.SystemTimeTag);

            TestWindow window = new TestWindow();
            window.Report.DataContext = this;
            window.DataContext = this;
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

        #region ViewModelsCommands
        public ICommand ShowAddTagCommand { get { return new RelayCommand(e => true, this.AddTagWindow); } }
        public ICommand AddTagCommand { get { return new RelayCommand(e => true, this.AddTag); } }
        public void AddTagWindow(object obj)
        {
            addTagView = new AddTagView();
            addTagView.DataContext = this;
            addTagView.Show();
        }
        public void AddTag(object obj)
        {
            if (addTagView.NewTagNameTextBox.Text.Length > 0 && addTagView.NewTagColorPicker.SelectedColorText.Length > 0)
                {
                SolidColorBrush brush = new SolidColorBrush((Color)addTagView.NewTagColorPicker.SelectedColor);
                _tags.Add(new TagModel() { TagColor = brush, TagName = addTagView.NewTagNameTextBox.Text, TagTime = new TimeSpan(0, 20, 0) });
                addTagView.Close();
            }
            else
                MessageBox.Show("Please insert tag name and tag color", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}
