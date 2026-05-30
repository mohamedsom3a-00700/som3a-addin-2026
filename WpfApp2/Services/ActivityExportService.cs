using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Services
{
    public class ActivityExportService : IActivityExportService
    {
        private readonly IServiceContainer _container;
        private Excel.Application? _xlApp;

        public ActivityExportService(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task ExportAsync(
            IReadOnlyList<GeneratedActivity> activities,
            ActivityExportConfig config,
            CancellationToken ct = default)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            EnsureExcelApp();

            if (_xlApp?.ActiveWorkbook == null)
                throw new InvalidOperationException("No active workbook found.");

            var workbook = _xlApp.ActiveWorkbook;

            if (SheetExists(config.TargetSheetName))
            {
                if (!config.OverwriteExisting)
                    throw new InvalidOperationException($"Sheet '{config.TargetSheetName}' already exists. Set OverwriteExisting to true to replace it.");

                RemoveSheet(config.TargetSheetName);
            }

            Excel.Worksheet? worksheet = null;
            try
            {
                var sheets = workbook.Sheets;
                var afterSheet = sheets[sheets.Count];
                var worksheets = workbook.Worksheets;
                worksheet = worksheets.Add(After: afterSheet);
                worksheet.Name = config.TargetSheetName;

                WriteHeaders(worksheet, config);
                WriteData(worksheet, activities, config);
                ApplyFormatting(worksheet, activities.Count, config.Columns.Length);

                var columns = worksheet.Columns;
                columns.AutoFit();
                if (columns != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(columns);
                if (afterSheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(afterSheet);
                if (worksheets != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheets);
                if (sheets != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(sheets);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export activities to Excel: {ex.Message}", ex);
            }
        }

        public bool SheetExists(string sheetName)
        {
            EnsureExcelApp();
            var activeWorkbook = _xlApp?.ActiveWorkbook;
            if (activeWorkbook == null) return false;

            var worksheets = activeWorkbook.Worksheets;
            try
            {
                foreach (Excel.Worksheet ws in worksheets)
                {
                    try
                    {
                        if (string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    finally
                    {
                        if (ws != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                    }
                }
            }
            finally
            {
                if (worksheets != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheets);
                if (activeWorkbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(activeWorkbook);
            }

            return false;
        }

        public void RemoveSheet(string sheetName)
        {
            EnsureExcelApp();
            var activeWorkbook = _xlApp?.ActiveWorkbook;
            if (activeWorkbook == null) return;

            var worksheets = activeWorkbook.Worksheets;
            try
            {
                foreach (Excel.Worksheet ws in worksheets)
                {
                    try
                    {
                        if (string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                        {
                            ws.Delete();
                            return;
                        }
                    }
                    finally
                    {
                        if (ws != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                    }
                }
            }
            finally
            {
                if (worksheets != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheets);
                if (activeWorkbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(activeWorkbook);
            }
        }

        private void EnsureExcelApp()
        {
            if (_xlApp == null)
            {
                try
                {
                    _xlApp = System.Runtime.InteropServices.Marshal.BindToMoniker("Excel.Application") as Excel.Application;
                }
                catch
                {
                    throw new InvalidOperationException("Excel is not running.");
                }
            }
        }

        private static void WriteHeaders(Excel.Worksheet worksheet, ActivityExportConfig config)
        {
            var columns = config.Columns;
            for (int i = 0; i < columns.Length; i++)
            {
                var header = columns[i] switch
                {
                    "ActivityId" => "Activity ID",
                    "BOQReference" => "BOQ Reference",
                    _ => columns[i]
                };

                worksheet.Cells[1, i + 1] = header;
            }
        }

        private void WriteData(Excel.Worksheet worksheet, IReadOnlyList<GeneratedActivity> activities, ActivityExportConfig config)
        {
            var columns = config.Columns;
            int row = 2;

            foreach (var activity in activities)
            {
                for (int col = 0; col < columns.Length; col++)
                {
                    object value = columns[col] switch
                    {
                        "ActivityId" => activity.ActivityId,
                        "Name" => activity.Name,
                        "Description" => activity.Description,
                        "BOQReference" => activity.BoqReferencesDisplay,
                        "Quantity" => activity.Quantity,
                        "Unit" => activity.Unit,
                        "Dependencies" => config.IncludeDependencies && activity.Dependencies?.Count > 0
                            ? string.Join(", ", activity.Dependencies)
                            : string.Empty,
                        _ => string.Empty
                    };

                    worksheet.Cells[row, col + 1] = value ?? string.Empty;
                }

                row++;
            }
        }

        private static void ApplyFormatting(Excel.Worksheet worksheet, int rowCount, int colCount)
        {
            if (rowCount <= 0) return;

            var headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, colCount]];
            headerRange.Font.Bold = true;
            headerRange.Interior.Color = System.Drawing.Color.FromArgb(55, 65, 75);
            headerRange.Font.Color = System.Drawing.Color.FromArgb(230, 230, 230);
            headerRange.Font.Size = 11;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            var dataRange = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[rowCount + 1, colCount]];
            dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            dataRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
            dataRange.Borders.Color = System.Drawing.Color.FromArgb(60, 60, 60);
            dataRange.Font.Color = System.Drawing.Color.FromArgb(200, 200, 200);
            dataRange.Font.Size = 10;

            worksheet.UsedRange.Columns.AutoFit();
            worksheet.UsedRange.RowHeight = 20;
        }
    }
}
