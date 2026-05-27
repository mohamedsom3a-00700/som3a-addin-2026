using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public class WBSEditorViewModel : INotifyPropertyChanged
{
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private readonly IWBSTemplateService _templateService;
    private WBSNode? _rootNode;
    private WBSNode? _selectedNode;
    private string? _newNodeName;
    private string? _errorMessage;
    private string? _statusMessage;
    private readonly Stack<WBSChange> _undoStack = new();
    private string? _currentUserId;

    public event PropertyChangedEventHandler? PropertyChanged;

    public WBSNode? RootNode
    {
        get => _rootNode;
        set { _rootNode = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasTree)); }
    }

    public WBSNode? SelectedNode
    {
        get => _selectedNode;
        set { _selectedNode = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddChild)); OnPropertyChanged(nameof(CanRemove)); }
    }

    public string? NewNodeName
    {
        get => _newNodeName;
        set { _newNodeName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddChild)); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool HasTree => RootNode != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool CanAddChild => SelectedNode != null && !string.IsNullOrWhiteSpace(NewNodeName);
    public bool CanRemove => SelectedNode != null && SelectedNode != RootNode;
    public bool CanUndo => _undoStack.Count > 0;

    public ICommand AddChildCommand { get; }
    public ICommand RemoveNodeCommand { get; }
    public ICommand RenameNodeCommand { get; }
    public ICommand SaveAsTemplateCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ExportToExcelCommand { get; }
    public ICommand ExportToJsonCommand { get; }
    public ICommand ExportToXmlCommand { get; }

    public WBSEditorViewModel(IWBSCodeGenerator codeGen, IWBSTreeValidator validator, IWBSTemplateService templateService)
    {
        _codeGen = codeGen;
        _validator = validator;
        _templateService = templateService;
        _currentUserId = Environment.UserName;

        AddChildCommand = new RelayCommand(() => AddChild(), () => CanAddChild);
        RemoveNodeCommand = new RelayCommand(() => RemoveNode(), () => CanRemove);
        RenameNodeCommand = new RelayCommand(() => RenameNode(SelectedNode?.Name), () => SelectedNode != null);
        SaveAsTemplateCommand = new RelayCommand(async () => await SaveAsTemplateAsync(), () => HasTree);
        UndoCommand = new RelayCommand(() => Undo(), () => CanUndo);
        ExportToExcelCommand = new RelayCommand(() => ExportToActiveSheet(), () => HasTree);
        ExportToJsonCommand = new RelayCommand(() => ExportToFile("json"), () => HasTree);
        ExportToXmlCommand = new RelayCommand(() => ExportToFile("xml"), () => HasTree);
    }

    public void LoadTree(WBSNode root)
    {
        RootNode = root;
        _undoStack.Clear();
        StatusMessage = "Tree loaded.";
    }

    private void AddChild()
    {
        if (SelectedNode == null || string.IsNullOrWhiteSpace(NewNodeName)) return;
        var child = new WBSNode { Name = NewNodeName, Parent = SelectedNode, Code = _codeGen.GetNextSiblingCode(SelectedNode) };
        var validation = _validator.ValidateNode(child, SelectedNode);
        if (!validation.IsValid) { ErrorMessage = string.Join("; ", validation.Errors); return; }
        SnapshotBeforeChange(child, "Add");
        SelectedNode.Children.Add(child);
        _codeGen.RenumberSubtree(RootNode!);
        NewNodeName = string.Empty; ErrorMessage = null; StatusMessage = "Node added.";
        RefreshProperties();
    }

    private void RemoveNode()
    {
        if (SelectedNode == null || SelectedNode == RootNode) return;
        var parent = SelectedNode.Parent;
        if (parent == null) return;
        SnapshotBeforeChange(SelectedNode, "Remove");
        parent.Children.Remove(SelectedNode);
        _codeGen.RenumberSubtree(RootNode!);
        SelectedNode = null; ErrorMessage = null; StatusMessage = "Node removed.";
        RefreshProperties();
    }

    public void RenameNode(string? newName)
    {
        if (SelectedNode == null || string.IsNullOrWhiteSpace(newName)) return;
        SnapshotBeforeChange(SelectedNode, "Rename");
        SelectedNode.Name = newName;
        StatusMessage = "Node renamed.";
        RefreshProperties();
    }

    private async Task SaveAsTemplateAsync()
    {
        if (RootNode == null) return;
        try
        {
            await _templateService.CreateCustomTemplateAsync($"Custom - {RootNode.Name}", "Custom", RootNode, _currentUserId ?? "unknown");
            StatusMessage = "Template saved.";
        }
        catch (Exception ex) { ErrorMessage = $"Failed to save template: {ex.Message}"; }
    }

    private void Undo()
    {
        if (_undoStack.Count == 0) return;
        var change = _undoStack.Pop();
        StatusMessage = $"Undo: {change.Type}";
        RefreshProperties();
    }

    private void SnapshotBeforeChange(WBSNode node, string changeType)
    {
        _undoStack.Push(new WBSChange { Id = Guid.NewGuid().ToString(), Type = changeType, NodeId = node.Id, Timestamp = DateTime.UtcNow });
    }

    private void RefreshProperties()
    {
        OnPropertyChanged(nameof(CanUndo)); OnPropertyChanged(nameof(HasTree));
        OnPropertyChanged(nameof(CanAddChild)); OnPropertyChanged(nameof(CanRemove));
    }

    private void ExportToActiveSheet()
    {
        if (RootNode == null) return;
        try
        {
            var service = new WBSExcelExportService();
            StatusMessage = "Pass Excel app object to export to active sheet.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
        }
    }

    private void ExportToFile(string format)
    {
        if (RootNode == null) return;
        try
        {
            var filter = format.ToLower() switch
            {
                "json" => "JSON files (*.json)|*.json",
                "xml" => "XML files (*.xml)|*.xml",
                _ => "All files (*.*)|*.*"
            };
            var ext = format.ToLower() == "json" ? ".json" : ".xml";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                FileName = $"WBS_{DateTime.Now:yyyyMMdd}{ext}"
            };
            if (dialog.ShowDialog() == true)
            {
                var exportSvc = new WBSExportService();
                if (format == "json")
                    exportSvc.ExportToJsonAsync(RootNode, dialog.FileName).GetAwaiter().GetResult();
                else
                    exportSvc.ExportToXmlAsync(RootNode, dialog.FileName).GetAwaiter().GetResult();
                StatusMessage = $"Exported to {dialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class WBSChange
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
