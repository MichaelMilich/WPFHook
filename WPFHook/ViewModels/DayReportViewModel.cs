using System;
using System.Collections.Generic;
using System.Text;
using WPFHook.Models;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    class DayReportViewModel
    {
        public DayReportModel model;
        private ReportWindow view;
        public ReportWindow View
        {
            get { return view; }
        }
        public DayReportViewModel()
        {
            view = new ReportWindow();
            view.Report.DataContext = model;
            view.DataContext = this;
        }
        public DayReportViewModel(DayReportModel model)
        {
            this.model = model;
            view = new ReportWindow();
            view.Report.DataContext = model;
            view.DataContext = model;
        }
        public void Show()
        {
            view.Show();
        }
    }
}
