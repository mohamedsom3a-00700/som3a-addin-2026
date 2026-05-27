using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services.WBS;
using Som3a_WPF_UI.ViewModels.WBS;

namespace Som3a_WPF_UI.Pages.WBS;

public partial class WBSEditorPage : Page
{
    private WBSEditorViewModel? _viewModel;

    public WBSEditorPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) return;
        var codeGen = new WBSCodeGenerator();
        var validator = new WBSTreeValidator();
        var templateSvc = new WBSTemplateService();
        _viewModel = new WBSEditorViewModel(codeGen, validator, templateSvc);
        DataContext = _viewModel;
    }

    private void OnSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_viewModel == null) return;
        _viewModel.SelectedNode = e.NewValue as WBSNode;
    }
}
