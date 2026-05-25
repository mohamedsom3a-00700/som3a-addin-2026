using Som3a.Shared.Core.Primavera;
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
        }

        public void InitializeWithResult(ComparisonResult result)
        {
            DataContext = new PrimaveraResultsViewModel(App.Container, result);
        }
    }
}
