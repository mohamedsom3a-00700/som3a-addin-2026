using System.Windows;
using System.Windows.Controls;
using Som3a.Domain.WBS;
using WpfApp2.Services.WBS;
using WpfApp2.ViewModels.WBS;

namespace WpfApp2.Pages.WBS;

public partial class WBSEditorPage : Page
{
    private readonly WBSEditorViewModel _viewModel;

    public WBSEditorPage(IWBSCodeGenerator codeGen, IWBSTreeValidator validator, IWBSTemplateService templateService)
    {
        InitializeComponent();
        _viewModel = new WBSEditorViewModel(codeGen, validator, templateService);
        DataContext = _viewModel;
    }

    private void OnSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is WBSNode node)
            _viewModel.SelectedNode = node;
    }
}
