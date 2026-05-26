using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Som3a.Domain.WBS;
using WpfApp2.Services.WBS;

namespace WpfApp2.ViewModels.WBS;

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
        set { _newNodeName = value; OnPropertyChanged(); }
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

    public WBSEditorViewModel(IWBSCodeGenerator codeGen, IWBSTreeValidator validator, IWBSTemplateService templateService)
    {
        _codeGen = codeGen;
        _validator = validator;
        _templateService = templateService;
        _currentUserId = Environment.UserName;

        AddChildCommand = new RelayCommand(_ => AddChild(), () => CanAddChild);
        RemoveNodeCommand = new RelayCommand(_ => RemoveNode(), () => CanRemove);
        RenameNodeCommand = new RelayCommand<string>(newName => RenameNode(newName), _ => SelectedNode != null);
        SaveAsTemplateCommand = new RelayCommandAsync(_ => SaveAsTemplateAsync());
        UndoCommand = new RelayCommand(_ => Undo(), () => CanUndo);
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

        var child = new WBSNode
        {
            Name = NewNodeName,
            Parent = SelectedNode,
            Code = _codeGen.GetNextSiblingCode(SelectedNode)
        };

        var validation = _validator.ValidateNode(child, SelectedNode);
        if (!validation.IsValid)
        {
            ErrorMessage = string.Join("; ", validation.Errors);
            return;
        }

        SnapshotBeforeChange(child, "Add");
        SelectedNode.Children.Add(child);
        _codeGen.RenumberSubtree(RootNode!);
        NewNodeName = string.Empty;
        ErrorMessage = null;
        StatusMessage = "Node added.";
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
        SelectedNode = null;
        ErrorMessage = null;
        StatusMessage = "Node removed.";
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
            var name = $"Custom - {RootNode.Name}";
            var category = "Custom";
            await _templateService.CreateCustomTemplateAsync(name, category, RootNode, _currentUserId ?? "unknown");
            StatusMessage = "Template saved.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save template: {ex.Message}";
        }
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
        _undoStack.Push(new WBSChange
        {
            Id = Guid.NewGuid().ToString(),
            Type = changeType,
            NodeId = node.Id,
            Timestamp = DateTime.UtcNow
        });
    }

    private void RefreshProperties()
    {
        OnPropertyChanged(nameof(RootNode));
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(HasTree));
        OnPropertyChanged(nameof(CanAddChild));
        OnPropertyChanged(nameof(CanRemove));
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
