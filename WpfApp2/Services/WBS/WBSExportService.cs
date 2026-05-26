using System.Text.Json;
using System.Xml.Linq;
using ClosedXML.Excel;
using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public class WBSExportService : IWBSExportService
{
    public Task ExportToExcelAsync(WBSNode root, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("WBS");

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Level";
        ws.Cell(1, 4).Value = "Full Path";

        var header = ws.Row(1);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        WriteExcelRow(ws, root, ref row, 0);

        ws.Columns().AdjustToContents();
        return Task.CompletedTask;
    }

    private static void WriteExcelRow(IXLWorksheet ws, WBSNode node, ref int row, int indent)
    {
        ws.Cell(row, 1).Value = node.Code;
        ws.Cell(row, 2).Value = node.Name;
        ws.Cell(row, 2).Style.Indentation = indent;
        ws.Cell(row, 3).Value = node.Level;
        ws.Cell(row, 4).Value = node.FullPath;
        row++;

        if (indent > 0)
            ws.Row(row - 1).GroupIndent();

        foreach (var child in node.Children)
            WriteExcelRow(ws, child, ref row, indent + 1);
    }

    public Task ExportToJsonAsync(WBSNode root, string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(root, options);
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
        var element = new XElement("WBSNode",
            new XAttribute("Code", node.Code),
            new XAttribute("Name", node.Name),
            new XAttribute("Level", node.Level));

        if (!string.IsNullOrEmpty(node.FullPath))
            element.Add(new XAttribute("FullPath", node.FullPath));

        foreach (var child in node.Children)
            element.Add(SerializeNode(child));

        return element;
    }
}
