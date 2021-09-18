using System;
using System.Collections.Generic;
using System.Text;
using WPFHook.Models;
using WPFHook.ViewModels.BackgroundLogic;
using WPFHook.Views;

namespace WPFHook.ViewModels
{
    class DayReportViewModel
    {
        public TagViewModel model;
        private ReportWindow view;
        private List<ActivityLine> data;
        public ReportWindow View
        {
            get { return view; }
        }
        public List<ActivityLine> Data
        {
            get { return data; }
        }
        public DayReportViewModel()
        {
            view = new ReportWindow();
            view.Report.DataContext = model;
            view.DataContext = this;
        }
        public DayReportViewModel(TagViewModel model , List<ActivityLine> data)
        {
            this.model = model;
            this.data = data;
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
