using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace WPFHook.Models
{
    public class RuleTagModel
    {
        private string parameter;
        private string operation;
        private string constant;
        private int ruleId;
        private string tagName;
        private Brush tagcolor;
        public int RuleId
        {
            get { return ruleId; }
            set 
            {
                ruleId = value;
                OnPropertyChanged("RuleId");
            }
        }
        public string Parameter
        {
            get { return parameter; }
            set
            {
                parameter = value;
                OnPropertyChanged("Parameter");
            }
        }
        public string Operator
        {
            get { return operation; }
            set
            { 
                operation = value;
                OnPropertyChanged("Operator");
            }
        }
        public string Constant
        {
            get { return constant; }
            set 
            {
                constant = value;
                OnPropertyChanged("Constant");
            }
        }
        public string TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;
                OnPropertyChanged("TagName");
            }
        }
        public Brush TagColor
        {
            get { return tagcolor; }
            set
            {
                tagcolor = value;
                OnPropertyChanged("TagColor");
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
