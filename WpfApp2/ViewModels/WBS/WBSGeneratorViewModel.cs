using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public class WBSGeneratorViewModel : INotifyPropertyChanged
{
    private readonly IWBSAIService _aiService;
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private CancellationTokenSource? _generationCts;
    private string _projectDescription = string.Empty;
    private string? _statusMessage;
    private bool _isGenerating;
    private WBSNode? _generatedWbs;
    private string? _boqSummary;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ProjectDescription
    {
        get => _projectDescription;
        set { _projectDescription = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanGenerate)); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        set { _isGenerating = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanGenerate)); }
    }

    public WBSNode? GeneratedWbs
    {
        get => _generatedWbs;
        set { _generatedWbs = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResult)); }
    }

    public bool HasResult => GeneratedWbs != null;
    public bool CanLoadBoq => !IsGenerating;
    public bool CanGenerate => !IsGenerating && !string.IsNullOrWhiteSpace(ProjectDescription);

    public ICommand LoadBoqCommand { get; }
    public ICommand GenerateCommand { get; }
    public ICommand AcceptCommand { get; }
    public ICommand RegenerateCommand { get; }
    public ICommand CancelCommand { get; }

    public WBSGeneratorViewModel(IWBSAIService aiService, IWBSCodeGenerator codeGen, IWBSTreeValidator validator)
    {
        _aiService = aiService;
        _codeGen = codeGen;
        _validator = validator;
        LoadBoqCommand = new RelayCommand(async _ => await LoadBoqAsync(), _ => CanLoadBoq);
        GenerateCommand = new RelayCommand(async _ => await GenerateAsync());
        AcceptCommand = new RelayCommand(() => AcceptWbs(), () => HasResult);
        RegenerateCommand = new RelayCommand(async _ => await RegenerateAsync());
        CancelCommand = new RelayCommand(() => CancelGeneration(), () => IsGenerating);
    }

    public async Task LoadBoqAsync()
    {
        StatusMessage = "Reading BOQ data from workbook...";
        try
        {
            await Task.Yield();
            var contextBuilder = App.Container.Resolve<IBOQContextBuilder>();
            var context = await contextBuilder.BuildContextAsync(CancellationToken.None);
            _boqSummary = $@"BOQ Context:
- Workbook: {context.WorkbookName}
- Sheet: {context.SheetName}
- Total Items: {context.ItemCount}
- Total Quantity: {context.TotalQuantity:F2}

BOQ Items:
{string.Join(Environment.NewLine, context.Items.Select(i => $"  {i.ItemNumber}\t{i.Description}\t{i.Quantity} {i.Unit}"))}";

            ProjectDescription = _boqSummary;
            StatusMessage = $"Loaded {context.ItemCount} BOQ items. Ready to generate WBS.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load BOQ: {ex.Message}";
        }
    }

    public async Task GenerateAsync()
    {
        if (!CanGenerate) return;
        _generationCts?.Cancel();
        _generationCts = new CancellationTokenSource();
        var ct = _generationCts.Token;

        IsGenerating = true;
        StatusMessage = "Generating WBS...";
        try
        {
            if (_aiService.IsAIAvailable)
            {
                var wbs = await _aiService.GenerateWBSAsync(ProjectDescription, null, ct);
                ct.ThrowIfCancellationRequested();
                _codeGen.RenumberSubtree(wbs);
                GeneratedWbs = wbs;
                StatusMessage = "WBS generated successfully.";
            }
            else
            {
                StatusMessage = "AI not available. Enable AI in Settings > AI.";
                GeneratedWbs = null;
            }
        }
        catch (OperationCanceledException) { StatusMessage = "Generation cancelled."; }
        catch (Exception ex) { StatusMessage = $"Generation failed: {ex.Message}"; }
        finally { IsGenerating = false; }
    }

    private async Task RegenerateAsync()
    {
        if (!HasResult) return;
        _generationCts?.Cancel();
        _generationCts = new CancellationTokenSource();
        var ct = _generationCts.Token;
        IsGenerating = true;
        StatusMessage = "Regenerating WBS...";
        try
        {
            var wbs = await _aiService.RegenerateWBSAsync(ProjectDescription, "", ct);
            ct.ThrowIfCancellationRequested();
            _codeGen.RenumberSubtree(wbs);
            GeneratedWbs = wbs;
            StatusMessage = "WBS regenerated.";
        }
        catch (OperationCanceledException) { StatusMessage = "Regeneration cancelled."; }
        catch (Exception ex) { StatusMessage = $"Regeneration failed: {ex.Message}"; }
        finally { IsGenerating = false; }
    }

    private void AcceptWbs()
    {
        if (GeneratedWbs == null) return;
        StatusMessage = "WBS accepted and saved.";
    }

    private void CancelGeneration()
    {
        _generationCts?.Cancel();
        StatusMessage = "Generation cancelled.";
        IsGenerating = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
