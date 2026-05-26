using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Som3a.Domain.WBS;
using WpfApp2.Services.WBS;

namespace WpfApp2.ViewModels.WBS;

public class WBSGeneratorViewModel : INotifyPropertyChanged
{
    private readonly IWBSAIService _aiService;
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private string _projectDescription = string.Empty;
    private string? _statusMessage;
    private bool _isGenerating;
    private WBSNode? _generatedWbs;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ProjectDescription
    {
        get => _projectDescription;
        set { _projectDescription = value; OnPropertyChanged(); }
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
    public bool CanGenerate => !IsGenerating && !string.IsNullOrWhiteSpace(ProjectDescription);

    public ICommand GenerateCommand { get; }
    public ICommand AcceptCommand { get; }
    public ICommand RegenerateCommand { get; }
    public ICommand CancelCommand { get; }

    public WBSGeneratorViewModel(IWBSAIService aiService, IWBSCodeGenerator codeGen, IWBSTreeValidator validator)
    {
        _aiService = aiService;
        _codeGen = codeGen;
        _validator = validator;
        GenerateCommand = new RelayCommandAsync(_ => GenerateAsync());
        AcceptCommand = new RelayCommand(_ => AcceptWbs(), () => HasResult);
        RegenerateCommand = new RelayCommandAsync(_ => RegenerateAsync());
        CancelCommand = new RelayCommand(_ => CancelGeneration(), () => IsGenerating);
    }

    public async Task GenerateAsync()
    {
        if (!CanGenerate) return;
        IsGenerating = true;
        StatusMessage = "Generating WBS...";

        try
        {
            if (_aiService.IsAIAvailable)
            {
                var wbs = await _aiService.GenerateWBSAsync(ProjectDescription);
                _codeGen.RenumberSubtree(wbs);
                GeneratedWbs = wbs;
                StatusMessage = "WBS generated successfully.";
            }
            else
            {
                StatusMessage = "AI not available. Switch to manual editor to build WBS manually.";
                GeneratedWbs = null;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Generation failed: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private async Task RegenerateAsync()
    {
        if (!HasResult) return;
        IsGenerating = true;
        StatusMessage = "Regenerating WBS...";

        try
        {
            var wbs = await _aiService.RegenerateWBSAsync(ProjectDescription, "");
            _codeGen.RenumberSubtree(wbs);
            GeneratedWbs = wbs;
            StatusMessage = "WBS regenerated.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Regeneration failed: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private void AcceptWbs()
    {
        if (GeneratedWbs == null) return;
        StatusMessage = "WBS accepted and saved.";
    }

    private void CancelGeneration()
    {
        StatusMessage = "Generation cancelled.";
        IsGenerating = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
