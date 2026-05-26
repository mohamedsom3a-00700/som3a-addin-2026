using System.Windows.Controls;
using WpfApp2.Services.WBS;
using WpfApp2.ViewModels.WBS;

namespace WpfApp2.Pages.WBS;

public partial class WBSTemplateBrowserPage : Page
{
    private readonly WBSTemplateBrowserViewModel _viewModel;

    public WBSTemplateBrowserPage()
    {
        InitializeComponent();
        var templateService = new WBSTemplateService();
        _viewModel = new WBSTemplateBrowserViewModel(templateService);
        DataContext = _viewModel;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.LoadTemplatesAsync();
    }
}
