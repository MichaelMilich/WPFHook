using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    public class TagModel : INotifyPropertyChanged
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
        public string TagColorString
        {
            get { return tagcolor.ToString(); }
        }
        public TagModel()
        {

        }
        public TagModel(string tag,TimeSpan time)
        {
            tagName = tag;
            tagTime = time;
        }
        public TagModel(string tag, TimeSpan time, Brush brush)
        {
            tagName = tag;
            tagTime = time;
            tagcolor = brush;
        }
        public TagModel(string tag, string brush)
        {
            tagName = tag;
            var color = (Color)ColorConverter.ConvertFromString(brush); // this assumes that the sting brush is in form of hex like "#FFDFD991"
            tagcolor = new SolidColorBrush(color);
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
