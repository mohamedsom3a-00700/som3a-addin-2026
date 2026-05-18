using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

public class ExcelService
{
    private readonly Excel.Application _app;

    public ExcelService(Excel.Application app)
    {
        _app = app;
    }

    // ================= EXPORT =================

    public void ExportTable(XerTable table)
    {
        if (_app == null)
            throw new Exception("Excel Application is null");

        // 🔥 أهم سطر
        if (_app.Workbooks.Count == 0)
            _app.Workbooks.Add();

        var wb = _app.ActiveWorkbook;

        if (wb == null)
            throw new Exception("No active workbook");

        var ws = wb.Worksheets.Add();

        ws.Name = table.Name.Length > 28
            ? table.Name.Substring(0, 28)
            : table.Name;

        // Headers
        for (int c = 0; c < table.Headers.Count; c++)
        {
            ws.Cells[1, c + 1] = table.Headers[c];
        }

        // Data
        for (int r = 0; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];

            if (row == null) continue;

            for (int c = 0; c < row.Count; c++)
            {
                ws.Cells[r + 2, c + 1] = row[c];
            }
        }
    }

    // ================= IMPORT =================

    public XerTable ReadTable(string sheetName)
    {
        Excel.Worksheet ws = null;

        foreach (Excel.Worksheet sheet in _app.ActiveWorkbook.Worksheets)
        {
            if (sheet.Name == sheetName)
            {
                ws = sheet;
                break;
            }
        }

        // 🔥 أهم شرط
        if (ws == null)
            return null;

        var table = new XerTable { Name = sheetName };

        int col = 1;

        // Headers
        while (ws.Cells[1, col].Value != null)
        {
            table.Headers.Add(ws.Cells[1, col].Value.ToString());
            col++;
        }

        int row = 2;

        while (ws.Cells[row, 1].Value != null)
        {
            var r = new List<string>();

            for (int c = 1; c <= table.Headers.Count; c++)
            {
                r.Add(ws.Cells[row, c].Value?.ToString());
            }

            table.Rows.Add(r);
            row++;
        }

        return table;
    }
    private Excel.Worksheet GetSheet(Excel.Workbook wb, string name)
    {
        foreach (Excel.Worksheet ws in wb.Worksheets)
        {
            if (ws.Name == name)
                return ws;
        }
        return null;
    }

}