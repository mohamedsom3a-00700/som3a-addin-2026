using Microsoft.Win32;
using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public class XerEditorViewModel : ViewModelBase
    {
        public ObservableCollection<TableItemVM> Tables { get; set; } = new();

        private readonly IServiceContainer _container;
        private XerParser _parser;
        private string _filePath = "";

        public RelayCommand LoadCommand { get; }
        public RelayCommand ExportExcelCommand { get; }
        public RelayCommand ImportFromExcelCommand { get; }
        public RelayCommand ExportXerCommand { get; }

        public XerEditorViewModel(IServiceContainer container)
        {
            _container = container;
            _parser = container.Resolve<XerParser>();

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

        private void Load()
        {
            var ofd = new OpenFileDialog { Filter = "XER (*.xer)|*.xer" };

            if (ofd.ShowDialog() != true) return;

            _filePath = ofd.FileName;

            _parser.Parse(_filePath);

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
                            existingSheet.Cells.Clear();

                            for (int c = 0; c < table.Headers.Count; c++)
                                existingSheet.Cells[1, c + 1] = table.Headers[c];

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

                MessageBox.Show("Export done.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.ToString());
            }
        }

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
                    Status = "Updated"
                });
            }
        }
        private void RefreshTablesUI(IEnumerable<TableItemVM> previous)
        {
            Tables.Clear();

            foreach (var t in _parser.Tables)
            {
                var prev = previous.FirstOrDefault(x => x.Name == t.Name);
                Tables.Add(new TableItemVM
                {
                    Name = t.Name,
                    DisplayName = GetDisplayName(t.Name),
                    Count = t.Rows.Count,
                    Status = prev is { Status: "Sheet Not Found" } ? "Sheet Not Found" : "Updated"
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

            app.ActiveWorkbook.Save();
            app.CalculateFullRebuild();

            var excel = new ExcelService(app);

            var selected = Tables.Where(x => x.IsSelected).ToList();

            foreach (var t in selected)
            {
                var updated = excel.ReadTable(t.Name);

                if (updated == null)
                {
                    t.Status = "Sheet Not Found";
                    continue;
                }

                var existing = _parser.Tables.FirstOrDefault(x => x.Name == t.Name);

                if (existing != null)
                    _parser.Tables.Remove(existing);

                _parser.Tables.Add(updated);
            }

            RefreshTablesUI(selected);

            foreach (var t in selected.Where(x => x.Status != "Sheet Not Found"))
            {
                var updated = Tables.FirstOrDefault(x => x.Name == t.Name);
                if (updated != null)
                {
                    updated.Count = _parser.Tables.FirstOrDefault(x => x.Name == t.Name)?.Rows.Count ?? 0;
                    updated.Status = "Updated";
                }
            }

            MessageBox.Show("Import completed");
        }

        private void ExportXer()
        {
            if (_parser == null || !_parser.Tables.Any())
            {
                MessageBox.Show("Load XER first");
                return;
            }

            var exporter = _container.Resolve<XerExportService>();

            exporter.Export(_filePath, _parser);

            MessageBox.Show("XER Exported with updated data");
        }
    }
}
