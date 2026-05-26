using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Som3a.Domain.WBS;
using WpfApp2.Services.WBS;

namespace WpfApp2.ViewModels.WBS;

public class WBSExportViewModel : INotifyPropertyChanged
{
    private readonly IWBSExportService _exportService;
    private WBSNode? _wbsRoot;
    private string _selectedFormat = "Excel";
    private string? _filePath;
    private string? _statusMessage;
    private bool _isExporting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string[] Formats { get; } = { "Excel", "JSON", "XML" };

    public string SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileExtension)); }
    }

    public string? FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanExport)); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsExporting
    {
        get => _isExporting;
        set { _isExporting = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanExport)); }
    }

    public bool CanExport => !IsExporting && !string.IsNullOrWhiteSpace(FilePath) && _wbsRoot != null;
    public string FileExtension => SelectedFormat.ToLower() switch
    {
        "excel" => ".xlsx",
        "json" => ".json",
        "xml" => ".xml",
        _ => ".dat"
    };

    public ICommand ExportCommand { get; }
    public ICommand BrowseFileCommand { get; }

    public WBSExportViewModel(IWBSExportService exportService)
    {
        _exportService = exportService;
        ExportCommand = new RelayCommandAsync(_ => ExportAsync());
        BrowseFileCommand = new RelayCommand(_ => BrowseFile());
    }

    public void SetWbsRoot(WBSNode root)
    {
        _wbsRoot = root;
        OnPropertyChanged(nameof(CanExport));
    }

    private void BrowseFile()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = SelectedFormat switch
            {
                "Excel" => "Excel files (*.xlsx)|*.xlsx",
                "JSON" => "JSON files (*.json)|*.json",
                "XML" => "XML files (*.xml)|*.xml",
                _ => "All files (*.*)|*.*"
            },
            FileName = $"WBS_{DateTime.Now:yyyyMMdd}{FileExtension}"
        };

        if (dialog.ShowDialog() == true)
            FilePath = dialog.FileName;
    }

    private async Task ExportAsync()
    {
        if (_wbsRoot == null || string.IsNullOrWhiteSpace(FilePath)) return;
        IsExporting = true;
        StatusMessage = $"Exporting to {SelectedFormat}...";

        try
        {
            switch (SelectedFormat)
            {
                case "Excel":
                    await _exportService.ExportToExcelAsync(_wbsRoot, FilePath);
                    break;
                case "JSON":
                    await _exportService.ExportToJsonAsync(_wbsRoot, FilePath);
                    break;
                case "XML":
                    await _exportService.ExportToXmlAsync(_wbsRoot, FilePath);
                    break;
            }
            StatusMessage = $"Exported successfully to {FilePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
