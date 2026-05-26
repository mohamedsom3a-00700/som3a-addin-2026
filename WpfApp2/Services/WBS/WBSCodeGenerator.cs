using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public class WBSCodeGenerator : IWBSCodeGenerator
{
    public string GenerateCode(WBSNode parent, int siblingIndex)
    {
        var prefix = parent.Code;
        return $"{prefix}.{siblingIndex + 1}";
    }

    public void RenumberNode(WBSNode node)
    {
        if (node.Parent == null)
        {
            var siblings = node.Parent?.Children ?? new List<WBSNode>();
            var index = siblings.IndexOf(node);
            node.Code = GenerateCode(new WBSNode { Code = "0" }, index).Substring(2);
            return;
        }

        var parent = node.Parent;
        var idx = parent.Children.IndexOf(node);
        node.Code = GenerateCode(parent, idx);
    }

    public void RenumberSubtree(WBSNode root)
    {
        int index = 1;
        RenumberSubtreeRecursive(root, ref index, null);
    }

    public string GetNextSiblingCode(WBSNode parent)
    {
        var count = parent.Children.Count;
        return GenerateCode(parent, count);
    }

    private void RenumberSubtreeRecursive(WBSNode node, ref int index, string? parentCode)
    {
        node.Code = parentCode == null ? index.ToString() : $"{parentCode}.{index}";

        int childIndex = 1;
        foreach (var child in node.Children)
        {
            RenumberSubtreeRecursive(child, ref childIndex, node.Code);
            childIndex++;
        }

        index++;
    }
}
