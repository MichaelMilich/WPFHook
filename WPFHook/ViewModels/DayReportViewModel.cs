using System;
using System.Collections.Generic;
using System.Text;
using WPFHook.Models;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    class DayReportViewModel
    {
        private DayReportModel model;
        private ReportWindow view;
        public DayReportModel Model
        {
            get { return model; }
        }
        public ReportWindow View
        {
            get { return view; }
        }

        public DayReportViewModel(DateTime Date, TimeSpan TotalTime, TimeSpan WorkTime, TimeSpan DistractionTime, TimeSpan SystemTime)
        {
            model = new DayReportModel();
            model.Date = Date;
            model.TotalTime = TotalTime;
            model.WorkTime = WorkTime;
            model.DistractionTime = DistractionTime;
            model.SystemTime = SystemTime;

            view = new ReportWindow();
            view.Report.DataContext = model;
            view.DataContext = this;

        }
        public void Show()
        {
            view.Show();
        }
    }
}
