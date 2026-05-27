namespace Som3a_WPF_UI.Services.WBS;

public enum WBSCodeMode { Numeric, Alpha }

public interface IWBSCodeGenerator
{
    WBSCodeMode Mode { get; set; }
    string GenerateCode(WBSNode parent, int siblingIndex);
    void RenumberNode(WBSNode node);
    void RenumberSubtree(WBSNode root, string? rootName = null);
    string GetNextSiblingCode(WBSNode parent);
}
