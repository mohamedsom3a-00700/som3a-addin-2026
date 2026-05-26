# Contract: IWBSCodeGenerator

**Layer**: WpfApp2.Services.WBS
**Purpose**: Hierarchical code auto-generation and renumbering on tree mutations.

```csharp
namespace WpfApp2.Services.WBS;

public interface IWBSCodeGenerator
{
    string GenerateCode(WBSNode parent, int siblingIndex);
    
    void RenumberNode(WBSNode node);
    
    void RenumberSubtree(WBSNode root);
    
    string GetNextSiblingCode(WBSNode parent);
}
```
