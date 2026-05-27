using System;
using Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSExcelExportService
{
    public void ExportToActiveSheet(WBSNode root, object excelApp, Worksheet? targetSheet = null)
    {
        var app = (Application)excelApp;
        var sheet = targetSheet ?? (Worksheet)app.ActiveSheet;

        // Write headers
        sheet.Cells[1, 1] = "Code";
        sheet.Cells[1, 2] = "Name";
        sheet.Cells[1, 3] = "Level";
        sheet.Cells[1, 4] = "Parent Code";
        sheet.Cells[1, 5] = "Full Path";

        var header = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, 5]];
        header.Font.Bold = true;
        header.Interior.Color = System.Drawing.Color.LightGray;

        int row = 2;
        WriteNodeToSheet(sheet, root, null, ref row);

        sheet.Columns.AutoFit();
    }

    private static void WriteNodeToSheet(Worksheet sheet, WBSNode node, WBSNode? parent, ref int row)
    {
        sheet.Cells[row, 1] = node.Code;
        sheet.Cells[row, 2] = node.Name;
        sheet.Cells[row, 3] = node.Level;
        sheet.Cells[row, 4] = parent?.Code ?? "";
        sheet.Cells[row, 5] = node.FullPath;

        // Indent name based on level
        var nameCell = sheet.Cells[row, 2] as Range;
        if (nameCell != null)
            nameCell.IndentLevel = Math.Min(node.Level, 15);

        row++;
        foreach (var child in node.Children)
            WriteNodeToSheet(sheet, child, node, ref row);
    }
}
