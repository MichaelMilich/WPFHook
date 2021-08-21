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
using WPFHook.ViewModels.BackgroundLogic;

namespace WPFHook.Views
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();

        }

        public TestWindow(string[,] test)
        {
            InitializeComponent();
            List<TTT> ts = new List<TTT>();
            for(int i=0;i<test.Length/2 -10 ;i++)
            {
                ts.Add(new TTT() { Name = test[0, i], Tag = test[1, i] });
            }
            dgUsers.ItemsSource = ts;
        }
    }
    public class TTT
    {
        public string Name { get; set; }
        public string Tag { get; set; }
    }
}
