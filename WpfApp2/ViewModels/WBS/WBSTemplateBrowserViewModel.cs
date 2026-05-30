using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public class WBSTemplateBrowserViewModel : ViewModelBase
{
    private readonly IWBSTemplateService _templateService;
    private WBSTemplateSummary? _selectedTemplate;
    private string? _activeCategory;
    private string? _projectContext;
    private WBSTemplate? _selectedTemplateDetail;
    private WBSNode? _previewRoot;
    private readonly IWBSCodeGenerator _codeGen;

    public ObservableCollection<WBSTemplateSummary> Templates { get; } = new();
    public ObservableCollection<string> Categories { get; } = new() { "All", "Building", "Infrastructure", "MEP", "Industrial", "Fitout" };

    public string? ActiveCategory
    {
        get => _activeCategory;
        set { _activeCategory = value; OnPropertyChanged(); _ = LoadTemplatesAsync(); }
    }

    public WBSTemplateSummary? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            _selectedTemplate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanPreview));
            _ = LoadTemplatePreviewAsync();
        }
    }

    public WBSNode? PreviewRoot
    {
        get => _previewRoot;
        set { _previewRoot = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPreview)); }
    }

    public bool HasPreview => PreviewRoot != null;
    public bool CanPreview => SelectedTemplate != null;

    public ICommand SelectTemplateCommand { get; }
    public ICommand LoadTemplatesCommand { get; }
    public ICommand GetRecommendationsCommand { get; }
    public ICommand ExportTemplateToExcelCommand { get; }
    public ICommand ImportTemplateFromExcelCommand { get; }

    public WBSTemplateBrowserViewModel(IWBSTemplateService templateService, IWBSCodeGenerator? codeGen = null)
    {
        _templateService = templateService;
        _codeGen = codeGen ?? new WBSCodeGenerator();
        SyncSelectedStyle();
        SelectTemplateCommand = new RelayCommand(() => { }, () => CanPreview);
        LoadTemplatesCommand = new RelayCommand(async _ => await LoadTemplatesAsync());
        GetRecommendationsCommand = new RelayCommand(async _ => await GetRecommendationsAsync());
        ExportTemplateToExcelCommand = new RelayCommand(async _ => await ExportTemplateToExcelAsync());
        ImportTemplateFromExcelCommand = new RelayCommand(async _ => await ImportTemplateFromExcelAsync());
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

    public async Task LoadTemplatesAsync()
    {
        var category = ActiveCategory == "All" ? null : ActiveCategory;
        var templates = await _templateService.ListTemplatesAsync(category);
        Templates.Clear();
        foreach (var t in templates) Templates.Add(t);
    }

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

    public string? ProjectContext
    {
        get => _projectContext;
        set { _projectContext = value; OnPropertyChanged(); }
    }

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
