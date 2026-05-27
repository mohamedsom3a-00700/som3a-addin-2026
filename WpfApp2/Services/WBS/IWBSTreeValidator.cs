using System.Collections.Generic;

namespace Som3a_WPF_UI.Services.WBS;

public interface IWBSTreeValidator
{
    ValidationResult ValidateTree(WBSNode root);
    ValidationResult ValidateNode(WBSNode node, WBSNode? newParent);
    bool DetectsCycle(WBSNode node, WBSNode potentialParent);
}

public class ValidationResult
{
    public bool IsValid { get; }
    public List<string> Errors { get; }

    public ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, new List<string>());
    public static ValidationResult Failure(string error) => new(false, new List<string> { error });
}
