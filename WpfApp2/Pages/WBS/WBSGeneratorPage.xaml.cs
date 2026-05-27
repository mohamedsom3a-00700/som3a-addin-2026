using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services.WBS;
using Som3a_WPF_UI.ViewModels.WBS;

namespace Som3a_WPF_UI.Pages.WBS;

public partial class WBSGeneratorPage : Page
{
    private WBSGeneratorViewModel? _viewModel;

    public WBSGeneratorPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) return;
        _viewModel = new WBSGeneratorViewModel(new WBSAIService(), new WBSCodeGenerator(), new WBSTreeValidator());
        DataContext = _viewModel;
    }
}
