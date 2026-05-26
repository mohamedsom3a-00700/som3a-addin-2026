using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public interface IWBSCodeGenerator
{
    string GenerateCode(WBSNode parent, int siblingIndex);
    void RenumberNode(WBSNode node);
    void RenumberSubtree(WBSNode root);
    string GetNextSiblingCode(WBSNode parent);
}
