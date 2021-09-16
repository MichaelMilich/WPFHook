using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFHook.Views
{
    /// <summary>
    /// Interaction logic for AddRuleView.xaml
    /// </summary>
    public partial class AddRuleView : Window
    {
        public AddRuleView()
        {
            InitializeComponent();
        }


        private void ruleOperators_DropDownClosed(object sender, EventArgs e)
        {
            if (ruleOperators.Text.Equals("Every thing else"))
            {
                constantTextBox.Text = "Every thing else";
                constantTextBox.IsReadOnly = true;
            }
            else
                constantTextBox.IsReadOnly = false;
        }
    }
}
