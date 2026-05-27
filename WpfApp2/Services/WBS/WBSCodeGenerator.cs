using System;
using System.Text;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSCodeGenerator : IWBSCodeGenerator
{
    public static WBSCodeMode DefaultMode { get; set; } = WBSCodeMode.Numeric;
    public WBSCodeMode Mode { get; set; } = DefaultMode;

    public string GenerateCode(WBSNode parent, int siblingIndex)
    {
        if (Mode == WBSCodeMode.Alpha)
            return $"{parent.Code}.{IndexToAlphaCode(siblingIndex)}";
        return $"{parent.Code}.{siblingIndex + 1}";
    }

    public static string NameToCode(string name, int minLength = 3)
    {
        var chars = new char[minLength];
        for (int i = 0; i < minLength; i++)
        {
            if (i < name.Length && char.IsLetter(name[i]))
                chars[i] = char.ToUpperInvariant(name[i]);
            else
                chars[i] = 'A';
        }
        return new string(chars);
    }

    public void RenumberNode(WBSNode node)
    {
        if (node.Parent == null)
        {
            node.Code = Mode == WBSCodeMode.Alpha ? "AAA" : "1";
            return;
        }

        var parent = node.Parent;
        var idx = parent.Children.IndexOf(node);
        node.Code = GenerateCode(parent, idx);
    }

    public void RenumberSubtree(WBSNode root, string? rootName = null)
    {
        if (Mode == WBSCodeMode.Numeric)
        {
            int index = 1;
            RenumberNumericRecursive(root, ref index, null);
        }
        else
        {
            root.Code = !string.IsNullOrWhiteSpace(rootName)
                ? NameToCode(rootName)
                : "AAA";
            int childIndex = 0;
            foreach (var child in root.Children)
                RenumberAlphaRecursive(child, root, ref childIndex);
        }
    }

    public string GetNextSiblingCode(WBSNode parent)
    {
        return GenerateCode(parent, parent.Children.Count);
    }

    private static void RenumberNumericRecursive(WBSNode node, ref int index, string? parentCode)
    {
        node.Code = parentCode == null ? index.ToString() : $"{parentCode}.{index}";
        int childIndex = 1;
        foreach (var child in node.Children)
            RenumberNumericRecursive(child, ref childIndex, node.Code);
        index++;
    }

    private static void RenumberAlphaRecursive(WBSNode node, WBSNode parent, ref int siblingIndex)
    {
        node.Code = $"{parent.Code}.{IndexToAlphaCode(siblingIndex)}";
        int childIndex = 0;
        foreach (var child in node.Children)
            RenumberAlphaRecursive(child, node, ref childIndex);
        siblingIndex++;
    }

    public static string IndexToAlphaCode(int index, int minLength = 3)
    {
        var sb = new StringBuilder();
        do
        {
            sb.Insert(0, (char)('A' + (index % 26)));
            index /= 26;
        } while (index > 0);

        while (sb.Length < minLength)
            sb.Insert(0, 'A');

        return sb.ToString();
    }
}