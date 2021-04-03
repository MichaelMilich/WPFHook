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

namespace WPFHook
{
    /// <summary>
    /// Interaction logic for ActivityDatabaseWindow.xaml
    /// </summary>
    public partial class ActivityDatabaseWindow : Window
    {
        public ActivityDatabaseWindow()
        {
            InitializeComponent();
        }
        public void ShowDataBase(List<ActivityLine> activities)
        {
            if (activities is null)
            {
                throw new ArgumentNullException(nameof(activities));
            }
            else
            {
                dgUsers.ItemsSource = activities;
            }

        }
    }
}
