using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public partial class WBSGeneratorViewModel : ViewModelBase
{
    private readonly IWBSAIService _aiService;
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private CancellationTokenSource? _generationCts;
    private string? _boqSummary;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    private string _projectDescription = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    [NotifyPropertyChangedFor(nameof(CanLoadBoq))]
    private bool _isGenerating;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    private WBSNode? _generatedWbs;

    public bool HasResult => GeneratedWbs != null;
    public bool CanLoadBoq => !IsGenerating;
    public bool CanGenerate => !IsGenerating && !string.IsNullOrWhiteSpace(ProjectDescription);

    public WBSGeneratorViewModel(IWBSAIService aiService, IWBSCodeGenerator codeGen, IWBSTreeValidator validator)
    {
        _aiService = aiService;
        _codeGen = codeGen;
        _validator = validator;
    }

    [RelayCommand(CanExecute = nameof(CanLoadBoq))]
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

    [RelayCommand]
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

    [RelayCommand(CanExecute = nameof(HasResult))]
    private void Accept()
    {
        if (GeneratedWbs == null) return;
        StatusMessage = "WBS accepted and saved.";
    }

    [RelayCommand]
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

    [RelayCommand(CanExecute = nameof(IsGenerating))]
    private void Cancel()
    {
        _generationCts?.Cancel();
        StatusMessage = "Generation cancelled.";
        IsGenerating = false;
    }
}
