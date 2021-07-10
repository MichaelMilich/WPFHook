using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    class TagModel : INotifyPropertyChanged
    {
        private string tagName;
        public string TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;
                OnPropertyChanged("TagName");
            }
        }
        private TimeSpan tagTime;
        public TimeSpan TagTime
        {
            get { return tagTime; }
            set
            {
                tagTime = value;
                OnPropertyChanged("TagTime");
            }
        }
        private Brush tagcolor;
        public Brush TagColor
        {
            get { return tagcolor; }
            set
            {
                tagcolor = value;
                OnPropertyChanged("TagColor");
            }
        }
        public TagModel()
        {

        }
        public TagModel(string tag,TimeSpan time)
        {
            tagName = tag;
            tagTime = time;
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
