using System;
using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services.WBS;
using Som3a_WPF_UI.ViewModels.WBS;

namespace Som3a_WPF_UI.Pages.WBS;

public partial class WBSTemplateBrowserPage : Page
{
    private WBSTemplateBrowserViewModel? _viewModel;

    public WBSTemplateBrowserPage()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) return;
        try
        {
            var templateService = new WBSTemplateService();
            _viewModel = new WBSTemplateBrowserViewModel(templateService);
            DataContext = _viewModel;
            await _viewModel.LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load templates: {ex.Message}", "WBS Templates", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
