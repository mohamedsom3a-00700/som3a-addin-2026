using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI
{
    public sealed class AssignProgress
    {
        public int Percent { get; set; }
        public string StatusText { get; set; } = "";
        public (int Row, string Activity, string Code, string Status)? LogItem { get; set; }
    }

    public sealed class AssignTradeCodesService
    {
        private const string RefActivityCol = "C";
        private const string RefCodeCol = "A";

        public void AssignTradeCodes(
            Excel.Worksheet ws,
            Excel.Worksheet wsRef,
            string targetColLetter,
            string activityColLetter,
            string statusColLetter,
            IProgress<AssignProgress>? progress,
            CancellationToken token)
        {
            long lastRowLeft = GetLastRow(ws, activityColLetter);
            long lastRowRight = GetLastRow(wsRef, RefActivityCol);

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Load reference
            for (int i = 2; i <= lastRowRight; i++)
            {
                token.ThrowIfCancellationRequested();

                var activityVal = CleanText(GetCellText(wsRef, RefActivityCol, i));
                var tradeCode = CleanText(GetCellText(wsRef, RefCodeCol, i));

                if (!string.IsNullOrWhiteSpace(activityVal) && !dict.ContainsKey(activityVal))
                    dict.Add(activityVal, tradeCode);
            }

            // Pre-clean main activity column
            for (int i = 2; i <= lastRowLeft; i++)
            {
                token.ThrowIfCancellationRequested();

                var cleaned = CleanText(GetCellText(ws, activityColLetter, i));
                SetCell(ws, activityColLetter, i, cleaned);
            }

            string lastAssigned = "";

            for (int i = 2; i <= lastRowLeft; i++)
            {
                token.ThrowIfCancellationRequested();

                var act = CleanText(GetCellText(ws, activityColLetter, i));

                string status;
                string codeToWrite = "";

                if (dict.TryGetValue(act, out var code))
                {
                    codeToWrite = code;
                    status = "Assigned";
                    lastAssigned = code;
                }
                else if (!string.IsNullOrWhiteSpace(lastAssigned))
                {
                    codeToWrite = lastAssigned;
                    status = "Filled Down";
                }
                else
                {
                    status = "Not Assigned";
                }

                if (!string.IsNullOrEmpty(codeToWrite))
                    SetCell(ws, targetColLetter, i, codeToWrite);

                SetCell(ws, statusColLetter, i, status);

                int pct = (int)Math.Round((i / (double)lastRowLeft) * 100.0);

                progress?.Report(new AssignProgress
                {
                    Percent = pct,
                    StatusText = $"Processing row {i} of {lastRowLeft}",
                    LogItem = (status == "Not Assigned")
                        ? null
                        : (i, act, codeToWrite, status)
                });
            }

            progress?.Report(new AssignProgress
            {
                Percent = 100,
                StatusText = "Completed"
            });
        }

        private static long GetLastRow(Excel.Worksheet ws, string colLetter)
            => ws.Cells[ws.Rows.Count, colLetter].End(Excel.XlDirection.xlUp).Row;

        private static string GetCellText(Excel.Worksheet ws, string colLetter, int row)
        {
            var v = ws.Range[$"{colLetter}{row}"].Value2;
            return v == null ? "" : Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
        }

        private static void SetCell(Excel.Worksheet ws, string colLetter, int row, string value)
            => ws.Range[$"{colLetter}{row}"].Value2 = value;

        public bool TryFindColumnInfo(Excel.Worksheet ws, string headerName, out string colLetter, out string colHeader)
        {
            colLetter = "";
            colHeader = "";

            var lastCol = ws.Cells[1, ws.Columns.Count].End(Excel.XlDirection.xlToLeft).Column;

            for (int c = 1; c <= lastCol; c++)
            {
                var cellVal = Convert.ToString(ws.Cells[1, c].Value2) ?? "";

                if (string.Equals(cellVal.Trim(), headerName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    colLetter = ColumnNumberToLetter(c);
                    colHeader = cellVal;
                    return true;
                }
            }

            return false;
        }

        public static string CleanText(string? v)
        {
            if (string.IsNullOrEmpty(v)) return "";

            var result = v.Replace("\r", "")
                          .Replace("\n", "")
                          .Replace("\t", " ")
                          .Trim();

            while (result.Contains("  "))
                result = result.Replace("  ", " ");

            return result;
        }

        private static string ColumnNumberToLetter(int col)
        {
            string s = "";
            while (col > 0)
            {
                int mod = (col - 1) % 26;
                s = (char)('A' + mod) + s;
                col = (col - mod) / 26;
            }
            return s;
        }
    }
}