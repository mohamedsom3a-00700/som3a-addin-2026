using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32;
using Som3a_WPF_UI.Services.WBS;

namespace Som3a_WPF_UI.ViewModels.WBS;

public partial class WBSEditorViewModel : ViewModelBase
{
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private readonly IWBSTemplateService _templateService;
    private readonly Stack<WBSChange> _undoStack = new();
    private string? _currentUserId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTree))]
    private WBSNode? _rootNode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddChild))]
    [NotifyPropertyChangedFor(nameof(CanRemove))]
    private WBSNode? _selectedNode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddChild))]
    private string? _newNodeName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasTree => RootNode != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool CanAddChild => SelectedNode != null && !string.IsNullOrWhiteSpace(NewNodeName);
    public bool CanRemove => SelectedNode != null && SelectedNode != RootNode;
    public bool CanUndo => _undoStack.Count > 0;

    public WBSEditorViewModel(IWBSCodeGenerator codeGen, IWBSTreeValidator validator, IWBSTemplateService templateService)
    {
        _codeGen = codeGen;
        _validator = validator;
        _templateService = templateService;
        _currentUserId = Environment.UserName;

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

    public void LoadTree(WBSNode root)
    {
        RootNode = root;
        _undoStack.Clear();
        StatusMessage = "Tree loaded.";
    }

    [RelayCommand(CanExecute = nameof(CanAddChild))]
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

    [RelayCommand(CanExecute = nameof(CanRemove))]
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

    [RelayCommand(CanExecute = nameof(CanRenameNode))]
    private void RenameNodeCommand()
    {
        RenameNode(SelectedNode?.Name);
    }

    private bool CanRenameNode() => SelectedNode != null;

    public void RenameNode(string? newName)
    {
        if (SelectedNode == null || string.IsNullOrWhiteSpace(newName)) return;
        SnapshotBeforeChange(SelectedNode, "Rename");
        SelectedNode.Name = newName;
        StatusMessage = "Node renamed.";
        RefreshProperties();
    }

    [RelayCommand(CanExecute = nameof(HasTree))]
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

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoStack.Count == 0) return;
        var change = _undoStack.Pop();
        StatusMessage = $"Undo: {change.Type}";
        RefreshProperties();
    }

    [RelayCommand(CanExecute = nameof(HasTree))]
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

    [RelayCommand(CanExecute = nameof(HasTree))]
    private void ExportToJson()
    {
        ExportToFile("json");
    }

    [RelayCommand(CanExecute = nameof(HasTree))]
    private void ExportToXml()
    {
        ExportToFile("xml");
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
            var dialog = new SaveFileDialog
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
}

public class WBSChange
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
