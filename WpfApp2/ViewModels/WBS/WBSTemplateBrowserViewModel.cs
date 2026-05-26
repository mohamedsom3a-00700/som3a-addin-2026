using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Som3a.Domain.WBS;
using WpfApp2.Services.WBS;

namespace WpfApp2.ViewModels.WBS;

public class WBSTemplateBrowserViewModel : INotifyPropertyChanged
{
    private readonly IWBSTemplateService _templateService;
    private WBSTemplateSummary? _selectedTemplate;
    private string? _activeCategory;
    private string? _projectContext;

    public event PropertyChangedEventHandler? PropertyChanged;

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
        set { _selectedTemplate = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanPreview)); }
    }

    public string? ProjectContext
    {
        get => _projectContext;
        set { _projectContext = value; OnPropertyChanged(); }
    }

    public bool CanPreview => SelectedTemplate != null;
    public ICommand LoadTemplatesCommand { get; }
    public ICommand FilterByCategoryCommand { get; }
    public ICommand GetRecommendationsCommand { get; }

    public WBSTemplateBrowserViewModel(IWBSTemplateService templateService)
    {
        _templateService = templateService;
        LoadTemplatesCommand = new RelayCommandAsync(_ => LoadTemplatesAsync());
        FilterByCategoryCommand = new RelayCommand<string>(category => ActiveCategory = category);
        GetRecommendationsCommand = new RelayCommandAsync(_ => GetRecommendationsAsync());
    }

    public async Task LoadTemplatesAsync()
    {
        var category = ActiveCategory == "All" ? null : ActiveCategory;
        var templates = await _templateService.ListTemplatesAsync(category);

        Templates.Clear();
        foreach (var t in templates)
            Templates.Add(t);
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

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    { _execute = execute; _canExecute = canExecute; }

    public bool CanExecute(object? _) => _canExecute?.Invoke() ?? true;
    public void Execute(object? _) => _execute();
}

internal class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    { _execute = execute; _canExecute = canExecute; }

    public bool CanExecute(object? p) => _canExecute?.Invoke((T?)p) ?? true;
    public void Execute(object? p) => _execute((T?)p);
}

internal class RelayCommandAsync : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;
    public event EventHandler? CanExecuteChanged;

    public RelayCommandAsync(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    { _execute = execute; _canExecute = canExecute; }

    public bool CanExecute(object? p) => !_isExecuting && (_canExecute?.Invoke(p) ?? true);

    public async void Execute(object? p)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        RaiseCanExecuteChanged();
        try { await _execute(p); }
        finally { _isExecuting = false; RaiseCanExecuteChanged(); }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
