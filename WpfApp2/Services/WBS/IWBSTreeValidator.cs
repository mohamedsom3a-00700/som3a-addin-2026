using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public interface IWBSTreeValidator
{
    ValidationResult ValidateTree(WBSNode root);
    ValidationResult ValidateNode(WBSNode node, WBSNode? newParent);
    bool DetectsCycle(WBSNode node, WBSNode potentialParent);
}

public record ValidationResult(bool IsValid, List<string> Errors)
{
    public static ValidationResult Success() => new(true, new());
    public static ValidationResult Failure(string error) => new(false, new() { error });
}
