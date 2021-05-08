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
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
        }
        public ReportWindow(DateTime Date, TimeSpan TotalTime, TimeSpan WorkTime, TimeSpan DistractionTime, TimeSpan SystemTime)
        {
            InitializeComponent();
            Report.Date = Date;
            Report.TotalTime = TotalTime;
            Report.WorkTime = WorkTime;
            Report.DistractionTime = DistractionTime;
            Report.SystemTime = SystemTime;
            Report.CalculateEfficiency();
        }
    }
}
