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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFHook
{
    /// <summary>
    /// Interaction logic for DayReport.xaml
    /// </summary>
    public partial class DayReport : UserControl
    {
        public DayReport()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public DateTime Date { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan WorkTime { get; set; }
        public TimeSpan DistractionTime { get; set; }
        public TimeSpan SystemTime { get; set; }
        public double Efficiency { get; set; }

        public DayReport(DateTime Date, TimeSpan TotalTime, TimeSpan WorkTime, TimeSpan DistractionTime, TimeSpan SystemTime)
        {
            InitializeComponent();
            this.Date = Date;
            this.TotalTime = TotalTime;
            this.WorkTime = WorkTime;
            this.DistractionTime = DistractionTime;
            this.SystemTime = SystemTime;
            CalculateEfficiency();
            this.DataContext = this;

        }
        public void CalculateEfficiency()
        {
            this.Efficiency = WorkTime.Divide(TotalTime);
        }
    }
}
