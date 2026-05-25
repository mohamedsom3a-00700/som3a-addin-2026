using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public partial class ProjectAnalysisPage : Page
    {
        public ProjectAnalysisPage()
        {
            InitializeComponent();
        }

        public void InitializeWithExcel(object excelApp)
        {
            var svc = new ExcelProjectAnalysisService(excelApp);
            DataContext = new ProjectAnalysisViewModel(App.Container, excelApp, null, svc);
        }
    }
}
