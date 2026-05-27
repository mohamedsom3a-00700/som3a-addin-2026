using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSExportService : IWBSExportService
{
    public Task ExportToExcelAsync(WBSNode root, string filePath)
    {
        var lines = new List<string>();
        lines.Add("Code\tName\tLevel\tParent Code\tFull Path");
        WriteCsvLines(root, null, 0, lines);
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    private static void WriteCsvLines(WBSNode node, WBSNode? parent, int indent, List<string> lines)
    {
        var indentStr = new string('\t', indent);
        lines.Add($"{indentStr}{node.Code}\t{node.Name}\t{node.Level}\t{parent?.Code ?? ""}\t{node.FullPath}");
        foreach (var child in node.Children)
            WriteCsvLines(child, node, indent + 1, lines);
    }

    public Task ExportToJsonAsync(WBSNode root, string filePath)
    {
        var json = JsonConvert.SerializeObject(root, Formatting.Indented);
        File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }

    public Task ExportToXmlAsync(WBSNode root, string filePath)
    {
        var doc = new XDocument(SerializeNode(root));
        doc.Save(filePath);
        return Task.CompletedTask;
    }

    private static XElement SerializeNode(WBSNode node)
    {
        var el = new XElement("WBSNode",
            new XAttribute("Code", node.Code),
            new XAttribute("Name", node.Name),
            new XAttribute("Level", node.Level));
        if (!string.IsNullOrEmpty(node.FullPath))
            el.Add(new XAttribute("FullPath", node.FullPath));
        foreach (var child in node.Children)
            el.Add(SerializeNode(child));
        return el;
    }
}
