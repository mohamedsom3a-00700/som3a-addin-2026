# Contract: IWBSTreeValidator

**Layer**: WpfApp2.Services.WBS
**Purpose**: Tree integrity validation on every mutation.

```csharp
namespace WpfApp2.Services.WBS;

public interface IWBSTreeValidator
{
    ValidationResult ValidateTree(WBSNode root);
    
    ValidationResult ValidateNode(WBSNode node, WBSNode? newParent);
    
    bool DetectsCycle(WBSNode node, WBSNode potentialParent);
}

public record ValidationResult(bool IsValid, List<string> Errors);
```
