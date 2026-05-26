using System.Windows.Controls;
using WpfApp2.Services.WBS;
using WpfApp2.ViewModels.WBS;

namespace WpfApp2.Pages.WBS;

public partial class WBSExportPage : Page
{
    public WBSExportPage(IWBSExportService exportService)
    {
        InitializeComponent();
        DataContext = new WBSExportViewModel(exportService);
    }
}
