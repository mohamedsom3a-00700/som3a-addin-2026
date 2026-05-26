using Som3a.Shared.Core.Primavera;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels.Primavera;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public partial class PrimaveraResultsPage : Page
    {
        public PrimaveraResultsPage()
        {
            InitializeComponent();

            var navData = Services.NavigationService.Instance.NavigationData;
            if (navData.TryGetValue("ComparisonResult", out var data) && data is ComparisonResult result)
            {
                navData.Remove("ComparisonResult");
                DataContext = new PrimaveraResultsViewModel(App.Container, result);
            }
        }
    }
}
