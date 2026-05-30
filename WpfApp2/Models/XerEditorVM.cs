using Microsoft.Win32;
using Som3a.Shared.Core;
using Som3a_WPF_UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
public class XerEditorVM
{
    public ObservableCollection<TableItemVM> Tables { get; set; } = new();

    private XerParser _parser = new();
    private string _filePath = "";

    public RelayCommand LoadCommand { get; }
    public RelayCommand ExportExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }
    public RelayCommand ExportXerCommand { get; }

    public XerEditorVM()
    {
        LoadCommand = new RelayCommand(Load);
        ExportExcelCommand = new RelayCommand(ExportExcel);
        ImportFromExcelCommand = new RelayCommand(ImportFromExcel);
        ExportXerCommand = new RelayCommand(ExportXer);
    }
    private string GetDisplayName(string tableName)
    {
        return tableName switch
        {
            "TASK" => "Activities",
            "TASKPRED" => "Relationships",
            "PROJWBS" => "Project WBS",
            "TASKRSRC" => "Resources",
            "RSRC" => "Resources List",
            "PROJECT" => "Project Info",
            _ => tableName
        };
    }

    // ================= LOAD =================

    private void Load()
    {
        var ofd = new OpenFileDialog { Filter = "XER (*.xer)|*.xer" };

        if (ofd.ShowDialog() != true) return;

        _filePath = ofd.FileName;

        _parser = new XerParser();
        _parser.Parse(_filePath);

        // 🔥 Reset كامل
        Tables.Clear();

        foreach (var t in _parser.Tables)
        {
            Tables.Add(new TableItemVM
            {
                Name = t.Name,
                DisplayName = GetDisplayName(t.Name),
                Count = t.Rows.Count,
                Status = "Loaded"
            });
        }
    }

    // ================= EXCEL INSTANCE =================

    private Excel.Application? GetExcel()
    {
        try
        {
            return (Excel.Application)System.Runtime.InteropServices.Marshal.BindToMoniker("Excel.Application");
        }
        catch
        {
            MessageBox.Show("Open Excel first");
            return null;
        }
    }

    // ================= EXPORT =================
    private string AskUserForAction(string sheetName)
    {
        var result = MessageBox.Show(
            $"Sheet '{sheetName}' already exists.\n\nYes = Replace\nNo = Update\nCancel = Stop",
            "Sheet Exists",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes) return "Replace";
        if (result == MessageBoxResult.No) return "Update";
        return "Cancel";
    }
    private Excel.Worksheet GetSheet(Excel.Workbook wb, string sheetName)
    {
        return wb.Worksheets
                 .Cast<Excel.Worksheet>()
                 .FirstOrDefault(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
    }
    private void ExportExcel()
    {
        try
        {
            if (_parser == null || !_parser.Tables.Any())
            {
                MessageBox.Show("Please load XER first");
                return;
            }

            var selected = Tables.Where(t => t.IsSelected).ToList();

            if (!selected.Any())
            {
                MessageBox.Show("Select at least one table");
                return;
            }

            var app = GetExcel();
            if (app == null) return;

            if (app.Workbooks.Count == 0)
                app.Workbooks.Add();

            var wb = app.ActiveWorkbook;
            var excel = new ExcelService(app);

            foreach (var t in selected)
            {
                var table = _parser.Tables.FirstOrDefault(x => x.Name == t.Name);
                if (table == null) continue;

                var existingSheet = GetSheet(wb, table.Name);

                if (existingSheet != null)
                {
                    var action = AskUserForAction(table.Name);

                    if (action == "Cancel")
                        return;

                    if (action == "Replace")
                    {
                        existingSheet.Delete();
                        excel.ExportTable(table);
                    }
                    else if (action == "Update")
                    {
                        // 🔥 تحديث داخل نفس الشيت
                        existingSheet.Cells.Clear();

                        // Headers
                        for (int c = 0; c < table.Headers.Count; c++)
                            existingSheet.Cells[1, c + 1] = table.Headers[c];

                        // Data
                        for (int r = 0; r < table.Rows.Count; r++)
                        {
                            for (int c = 0; c < table.Rows[r].Count; c++)
                            {
                                existingSheet.Cells[r + 2, c + 1] = table.Rows[r][c];
                            }
                        }
                    }
                }
                else
                {
                    excel.ExportTable(table);
                }
            }

            MessageBox.Show("Export done ✔");
        }
        catch (Exception ex)
        {
            MessageBox.Show("ERROR:\n" + ex.ToString());
        }
    }

    // ================= IMPORT =================
    private void RefreshTablesUI()
    {
        Tables.Clear();

        foreach (var t in _parser.Tables)
        {
            Tables.Add(new TableItemVM
            {
                Name = t.Name,
                DisplayName = GetDisplayName(t.Name),
                Count = t.Rows.Count,
                Status = "Updated ✔"
            });
        }
    }
    private void ImportFromExcel()
    {
        if (_parser == null || !_parser.Tables.Any())
        {
            MessageBox.Show("Please load XER first");
            return;
        }
        if (!Tables.Any(t => t.IsSelected))
        {
            MessageBox.Show("Please select at least one table from the list");
            return;
        }
        var app = GetExcel();
        if (app == null) return;

        // 🔥 أهم حاجة
        app.ActiveWorkbook.Save();
        app.CalculateFullRebuild();

        var excel = new ExcelService(app);

        foreach (var t in Tables.Where(x => x.IsSelected))
        {
            var updated = excel.ReadTable(t.Name);

            if (updated == null)
            {
                t.Status = "Sheet Not Found ❌";
                continue;
            }

            var existing = _parser.Tables.FirstOrDefault(x => x.Name == t.Name);

            // ✔ دايمًا استبدال
            if (existing != null)
                _parser.Tables.Remove(existing);

            _parser.Tables.Add(updated);
            RefreshTablesUI();
            t.Count = updated.Rows.Count;
            t.Status = "Updated ✔";
        }

        MessageBox.Show("Import completed ✔");
    }

    // ================= EXPORT XER =================

    private void ExportXer()
    {
        if (_parser == null || !_parser.Tables.Any())
        {
            MessageBox.Show("Load XER first");
            return;
        }

        var exporter = new XerExportService();

        exporter.Export(_filePath, _parser); // ✔ نفس instance

        MessageBox.Show("XER Exported with updated data");
    }
}