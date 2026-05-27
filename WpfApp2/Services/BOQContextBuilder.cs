using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Services
{
    public class BOQContextBuilder : IBOQContextBuilder
    {
        private const int MaxItemTokenLimit = 200;
        private static readonly string[] BoqSheetPatterns = { "BOQ", "Bill of Quantities", "BOQ ", "BillOfQuantities" };

        private readonly IServiceContainer _container;
        private Excel.Application? _xlApp;

        public BOQContextBuilder(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public bool TryIdentifyBoqSheet(out string? sheetName)
        {
            sheetName = null;
            EnsureExcelApp();

            if (_xlApp?.ActiveWorkbook == null)
                return false;

            foreach (Excel.Worksheet ws in _xlApp.ActiveWorkbook.Worksheets)
            {
                foreach (var pattern in BoqSheetPatterns)
                {
                    if (ws.Name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sheetName = ws.Name;
                        return true;
                    }
                }
            }

            if (_xlApp.ActiveWorkbook.Worksheets.Count > 0)
            {
                sheetName = ((Excel.Worksheet)_xlApp.ActiveWorkbook.Worksheets[1]).Name;
                return true;
            }

            return false;
        }

        public async Task<BOQContext> BuildContextAsync(CancellationToken ct = default)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            EnsureExcelApp();

            if (_xlApp?.ActiveWorkbook == null)
                throw new InvalidOperationException("No active workbook found.");

            if (!TryIdentifyBoqSheet(out var sheetName) || sheetName == null)
                throw new InvalidOperationException("Could not identify BOQ sheet in the active workbook.");

            var worksheet = (Excel.Worksheet?)_xlApp.ActiveWorkbook.Sheets[sheetName];
            if (worksheet == null)
                throw new InvalidOperationException($"Sheet '{sheetName}' not found.");

            var usedRange = worksheet.UsedRange;
            if (usedRange == null || usedRange.Rows.Count < 2)
                throw new InvalidOperationException("BOQ sheet has insufficient data.");

            ct.ThrowIfCancellationRequested();

            var items = ParseBoqItems(worksheet, usedRange);
            var totalQuantity = items.Sum(i => i.Quantity);
            var isTruncated = items.Count > MaxItemTokenLimit;
            var truncatedCount = 0;

            if (isTruncated)
            {
                truncatedCount = items.Count - MaxItemTokenLimit;
                items = items.Take(MaxItemTokenLimit).ToList();
            }

            return new BOQContext
            {
                WorkbookName = _xlApp.ActiveWorkbook.Name,
                SheetName = sheetName,
                Items = items,
                TotalQuantity = totalQuantity,
                IsTruncated = isTruncated,
                TruncatedItemCount = truncatedCount
            };
        }

        private static List<BOQItem> ParseBoqItems(Excel.Worksheet worksheet, Excel.Range usedRange)
        {
            var items = new List<BOQItem>();
            var rowCount = usedRange.Rows.Count;
            var colCount = Math.Min(usedRange.Columns.Count, 5);

            for (int row = 2; row <= rowCount; row++)
            {
                var itemNumber = GetCellValue(usedRange.Cells[row, 1]);
                var description = GetCellValue(usedRange.Cells[row, Math.Min(2, colCount)]);
                var quantityStr = GetCellValue(usedRange.Cells[row, Math.Min(3, colCount)]);
                var unit = GetCellValue(usedRange.Cells[row, Math.Min(4, colCount)]);
                var classification = colCount >= 5 ? GetCellValue(usedRange.Cells[row, 5]) : string.Empty;

                if (string.IsNullOrWhiteSpace(itemNumber) && string.IsNullOrWhiteSpace(description))
                    continue;

                decimal quantity = 0;
                decimal.TryParse(quantityStr, out quantity);

                if (quantity < 0)
                    continue;

                items.Add(new BOQItem
                {
                    ItemNumber = itemNumber ?? string.Empty,
                    Description = description ?? string.Empty,
                    Quantity = quantity,
                    Unit = unit ?? string.Empty,
                    Classification = classification ?? string.Empty,
                    Identifier = itemNumber
                });
            }

            return items;
        }

        private static string GetCellValue(object cell)
        {
            if (cell == null)
                return string.Empty;

            try
            {
                var value = ((Excel.Range)cell).Value2;
                return value?.ToString()?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void EnsureExcelApp()
        {
            if (_xlApp == null)
            {
                try
                {
                    _xlApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application") as Excel.Application;
                }
                catch
                {
                    throw new InvalidOperationException("Excel is not running. Please open Excel first.");
                }
            }
        }
    }
}
