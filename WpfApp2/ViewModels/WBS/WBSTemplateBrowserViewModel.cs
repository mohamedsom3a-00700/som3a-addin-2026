using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public partial class WBSTemplateBrowserViewModel : ViewModelBase
{
    private readonly IWBSTemplateService _templateService;
    private readonly IWBSCodeGenerator _codeGen;
    private WBSTemplate? _selectedTemplateDetail;

    public ObservableCollection<WBSTemplateSummary> Templates { get; } = new();
    public ObservableCollection<string> Categories { get; } = new() { "All", "Building", "Infrastructure", "MEP", "Industrial", "Fitout" };

    [ObservableProperty]
    private string? _activeCategory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPreview))]
    private WBSTemplateSummary? _selectedTemplate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreview))]
    private WBSNode? _previewRoot;

    [ObservableProperty]
    private string? _projectContext;

    partial void OnActiveCategoryChanged(string? value)
    {
        _ = LoadTemplatesAsync();
    }

    partial void OnSelectedTemplateChanged(WBSTemplateSummary? value)
    {
        _ = LoadTemplatePreviewAsync();
    }

    public bool HasPreview => PreviewRoot != null;
    public bool CanPreview => SelectedTemplate != null;

    public WBSTemplateBrowserViewModel(IWBSTemplateService templateService, IWBSCodeGenerator? codeGen = null)
    {
        _templateService = templateService;
        _codeGen = codeGen ?? new WBSCodeGenerator();
        SyncSelectedStyle();
    }

    private static void SyncSelectedStyle()
    {
        var styleName = Properties.Settings.Default.WBSExportStyle;
        Som3a.Shared.Core.UserSettings.SelectedStyle = styleName switch
        {
            "Blue Gradient" => 2,
            "Primavera" => 3,
            "Dark Mode" => 4,
            "Soft Pastel" => 5,
            _ => 1
        };
    }

    [RelayCommand(CanExecute = nameof(CanPreview))]
    private void SelectTemplate()
    {
    }

    [RelayCommand]
    public async Task LoadTemplatesAsync()
    {
        var category = ActiveCategory == "All" ? null : ActiveCategory;
        var templates = await _templateService.ListTemplatesAsync(category);
        Templates.Clear();
        foreach (var t in templates) Templates.Add(t);
    }

    [RelayCommand]
    private async Task GetRecommendationsAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectContext)) return;
        var recommended = _templateService.GetRecommendedTemplates(ProjectContext);
        var ids = recommended.Select(r => r.Id).ToHashSet();
        var all = await _templateService.ListTemplatesAsync(null);
        Templates.Clear();
        foreach (var t in all.OrderByDescending(t => ids.Contains(t.Id)))
            Templates.Add(t);
    }

    private async Task LoadTemplatePreviewAsync()
    {
        if (_selectedTemplate == null) { PreviewRoot = null; return; }
        try
        {
            _selectedTemplateDetail = await _templateService.GetTemplateAsync(_selectedTemplate.Id);
            PreviewRoot = _selectedTemplateDetail.RootNode;
            if (PreviewRoot != null)
                _codeGen.RenumberSubtree(PreviewRoot, _selectedTemplateDetail.Name);
        }
        catch
        {
            PreviewRoot = null;
        }
    }

    [RelayCommand]
    private async Task ExportTemplateToExcelAsync()
    {
        if (_selectedTemplate == null) return;
        try
        {
            var excelApp = Marshal.BindToMoniker("Excel.Application");
            await _templateService.ExportTemplateToExcelAsync(_selectedTemplate.Id, excelApp);
            System.Windows.MessageBox.Show($"Template '{_selectedTemplate.Name}' exported to new Excel sheet.",
                "WBS Templates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Export failed: {ex.Message}",
                "WBS Templates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task ImportTemplateFromExcelAsync()
    {
        try
        {
            var templateName = string.Empty;
            var category = ActiveCategory == "All" || string.IsNullOrEmpty(ActiveCategory) ? "Custom" : ActiveCategory;

            try
            {
                var excelApp = Marshal.BindToMoniker("Excel.Application");
                var imported = await _templateService.ImportTemplateFromExcelAsync(excelApp, null, category);
                templateName = imported.Name;
            }
            catch (Exception inner)
            {
                System.Windows.MessageBox.Show($"Import failed: {inner.Message}",
                    "WBS Templates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            await LoadTemplatesAsync();
            System.Windows.MessageBox.Show($"Template '{templateName}' imported to category '{category}'.",
                "WBS Templates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Import failed: {ex.Message}",
                "WBS Templates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }
}
