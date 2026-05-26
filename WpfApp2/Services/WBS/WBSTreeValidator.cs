using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public class WBSTreeValidator : IWBSTreeValidator
{
    private readonly int _maxDepth;

    public WBSTreeValidator(int maxDepth = 10)
    {
        _maxDepth = maxDepth;
    }

    public ValidationResult ValidateTree(WBSNode root)
    {
        var errors = new List<string>();
        var visited = new HashSet<string>();

        if (!ValidateNodeRecursive(root, 0, visited, errors))
            return ValidationResult.Failure("Tree contains a cycle.");

        if (errors.Count > 0)
            return new ValidationResult(false, errors);

        return ValidationResult.Success();
    }

    public ValidationResult ValidateNode(WBSNode node, WBSNode? newParent)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(node.Name))
            errors.Add("Node name must not be empty.");

        if (newParent != null && DetectsCycle(node, newParent))
            errors.Add("Cannot reparent: creates a cycle.");

        if (newParent != null && GetLevel(newParent) >= _maxDepth)
            errors.Add($"Maximum tree depth ({_maxDepth}) exceeded.");

        return errors.Count > 0 ? new ValidationResult(false, errors) : ValidationResult.Success();
    }

    public bool DetectsCycle(WBSNode node, WBSNode potentialParent)
    {
        var current = potentialParent;
        while (current != null)
        {
            if (current.Id == node.Id)
                return true;
            current = current.Parent;
        }
        return false;
    }

    private bool ValidateNodeRecursive(WBSNode node, int depth, HashSet<string> visited, List<string> errors)
    {
        if (!visited.Add(node.Id))
            return false;

        if (string.IsNullOrWhiteSpace(node.Name))
            errors.Add($"Node {node.Code}: name is empty.");

        if (depth > _maxDepth)
            errors.Add($"Node {node.Code}: exceeds max depth ({_maxDepth}).");

        foreach (var child in node.Children)
        {
            if (!ValidateNodeRecursive(child, depth + 1, visited, errors))
                return false;
        }

        return true;
    }

    private static int GetLevel(WBSNode node)
    {
        var level = 0;
        var current = node.Parent;
        while (current != null)
        {
            level++;
            current = current.Parent;
        }
        return level;
    }
}
