using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class SubDailyReportPage : Page
    {
        private Excel.Application _app;

        public SubDailyReportPage()
        {
            InitializeComponent();
        }

        public void InitializeWithExcel(Excel.Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            DataContext = new SubDailyReportViewModel(App.Container, _app, () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack());
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>((DependencyObject)sender);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is T t)
                    return t;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
