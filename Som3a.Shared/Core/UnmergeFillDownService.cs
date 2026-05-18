using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a.Shared.Core
{
    public sealed class UnmergeFillDownService
    {
        public IEnumerable<string> GetUsedColumnLetters(Excel.Worksheet ws, int headerRow)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            if (headerRow < 1) headerRow = 1;

            var result = new List<string>();

            Excel.Range? headerRange = null;
            try
            {
                int lastCol = ws.Cells[headerRow, ws.Columns.Count].End[Excel.XlDirection.xlToLeft].Column;
                if (lastCol < 1) return result;

                for (int c = 1; c <= lastCol; c++)
                {
                    object v = ws.Cells[headerRow, c].Value2;
                    bool hasHeader = v != null && v.ToString()!.Trim().Length > 0;

                    // VBA logic: Or CountA(ws.Columns(c)) > 0
                    int countA = 0;
                    Excel.Range col = ws.Columns[c];
                    try
                    {
                        countA = (int)ws.Application.WorksheetFunction.CountA(col);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(col);
                    }

                    if (hasHeader || countA > 0)
                        result.Add(ColumnLetter(c));
                }

                return result;
            }
            finally
            {
                if (headerRange != null) Marshal.ReleaseComObject(headerRange);
            }
        }

        public void UnmergeAndFillDownColumn(
            Excel.Worksheet ws,
            string colLetter,
            int startRow = 2,
            IProgress<double>? progress = null,
            CancellationToken token = default)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            if (string.IsNullOrWhiteSpace(colLetter)) throw new ArgumentException("Column letter is required.", nameof(colLetter));
            if (startRow < 1) startRow = 1;

            Excel.Range? colRange = null;

            try
            {
                colLetter = colLetter.Trim().ToUpperInvariant();

                colRange = ws.Columns[colLetter];
                colRange.UnMerge();

                int lastRow = ws.Cells[ws.Rows.Count, colLetter].End[Excel.XlDirection.xlUp].Row;
                if (lastRow < startRow)
                {
                    progress?.Report(100);
                    return;
                }

                int total = lastRow - startRow + 1;
                for (int r = startRow; r <= lastRow; r++)
                {
                    token.ThrowIfCancellationRequested();

                    Excel.Range cell = ws.Range[colLetter + r];
                    try
                    {
                        object v = cell.Value2;
                        if (v == null || v.ToString()!.Trim().Length == 0)
                        {
                            Excel.Range prev = ws.Range[colLetter + (r - 1)];
                            try
                            {
                                cell.Value2 = prev.Value2;
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(prev);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(cell);
                    }

                    double p = ((r - startRow + 1) * 100.0) / total;
                    progress?.Report(p);
                }

                progress?.Report(100);
            }
            finally
            {
                if (colRange != null) Marshal.ReleaseComObject(colRange);
            }
        }

        private static string ColumnLetter(int colNum)
        {
            // Equivalent to VBA: Split(Cells(1, colNum).Address(True, False), "$")(0)
            int dividend = colNum;
            string columnName = string.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}
