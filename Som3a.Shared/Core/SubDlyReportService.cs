using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a.Shared.Core
{
    public sealed class SubDlyReportService
    {
        public const string SubSheetPattern = "subcontractor";

        public List<string> GetSheetNames(Excel.Workbook wb)
        {
            var list = new List<string>();
            foreach (Excel.Worksheet ws in wb.Worksheets)
            {
                list.Add(ws.Name);
                Marshal.ReleaseComObject(ws);
            }
            return list;
        }
        private double SumMergedBlock(
            Excel.Worksheet ws,
            int startRow,
            int endRow,
            string countColLetter,
            int offset)
                {
                    double sum = 0;

                    for (int r = startRow; r <= endRow; r++)
                    {
                        int countRow = r + offset;
                        var cell = (Excel.Range)ws.Cells[countRow, countColLetter];
                        sum += ToDouble(cell.Value2);
                    }

                    return sum;
        }
        public void ApplyToNewSheet(
    Excel.Workbook wb,
    Excel.Worksheet sourceSheet,
    Dictionary<string, (double prev, double today)> merged,
    string prevTopLeftAddr)
        {
            if (merged.Count == 0) return;

            // 🟢 إنشاء شيت جديد فاضي
            var newSheet = (Excel.Worksheet)wb.Worksheets.Add(After: sourceSheet);

            // 🟢 اسم الشيت
            string baseName = "Daily_Report_" + DateTime.Now.ToString("ddMMyy");
            string newName = baseName;
            int i = 1;

            while (SheetExists(wb, newName))
            {
                newName = baseName + "_" + i;
                i++;
            }

            newSheet.Name = newName;

            // 🟢 نبدأ الكتابة من أول الصف
            int headerRow = 1;

            int colTL = 1;
            int colTC = 2;
            int colPL = 3;
            int colPC = 4;
            int colVar = 5;

            // 🟢 Headers
            newSheet.Cells[headerRow, colTL].Value2 = "Today - Labor";
            newSheet.Cells[headerRow, colTC].Value2 = "Today - Count";
            newSheet.Cells[headerRow, colPL].Value2 = "Previous - Labor";
            newSheet.Cells[headerRow, colPC].Value2 = "Previous - Count";
            newSheet.Cells[headerRow, colVar].Value2 = "Var";

            int rOut = headerRow + 1;

            foreach (var kv in merged)
            {
                if (IsTotalLikeName(kv.Key)) continue;

                newSheet.Cells[rOut, colTL].Value2 = kv.Key;
                newSheet.Cells[rOut, colTC].Value2 = kv.Value.today;
                newSheet.Cells[rOut, colPL].Value2 = kv.Key;
                newSheet.Cells[rOut, colPC].Value2 = kv.Value.prev;
                newSheet.Cells[rOut, colVar].Value2 = kv.Value.today - kv.Value.prev;

                rOut++;
            }

            // 🟢 Totals
            newSheet.Cells[rOut, colTL].Value2 = "Totals";
            newSheet.Cells[rOut, colPL].Value2 = "Totals";

            var rngToday = newSheet.Range[newSheet.Cells[2, colTC], newSheet.Cells[rOut - 1, colTC]];
            var rngPrev = newSheet.Range[newSheet.Cells[2, colPC], newSheet.Cells[rOut - 1, colPC]];

            newSheet.Cells[rOut, colTC].Formula = "=SUM(" + rngToday.Address[false, false] + ")";
            newSheet.Cells[rOut, colPC].Formula = "=SUM(" + rngPrev.Address[false, false] + ")";
            newSheet.Cells[rOut, colVar].Formula =
                newSheet.Cells[rOut, colTC].Address[false, false] + "-" +
                newSheet.Cells[rOut, colPC].Address[false, false];

            // 🟢 شوية تنسيق بسيط
            newSheet.Rows[1].Font.Bold = true;
            int lastRow = rOut;
            int lastCol = colVar;

            // 🟢 AutoFit + Minimum Width
            for (int c = 1; c <= lastCol; c++)
            {
                var col = (Excel.Range)newSheet.Columns[c];
                col.AutoFit();

                if (col.ColumnWidth < 20)
                    col.ColumnWidth = 20;
            }

            // 🟢 تحديد الرينجات
            var headerRange = newSheet.Range[newSheet.Cells[1, 1], newSheet.Cells[1, lastCol]];
            var dataRange = newSheet.Range[newSheet.Cells[2, 1], newSheet.Cells[lastRow - 1, lastCol]];
            var totalRange = newSheet.Range[newSheet.Cells[lastRow, 1], newSheet.Cells[lastRow, lastCol]];

            // 🟢 ألوان
            headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 32, 96));
            headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 255, 0));

            totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(0, 32, 96));
            totalRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 255, 0));

            // 🟢 Bold
            headerRange.Font.Bold = true;
            totalRange.Font.Bold = true;
            dataRange.Font.Bold = true;

            // 🟢 Borders - Header & Totals (full border)
            headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            totalRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

            // 🟢 Borders - Data
            var borders = dataRange.Borders;

            // Left & Right
            borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;

            // Top & Bottom (dotted)
            borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlDot;
            borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlDot;
            borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlDot;

            // 🟢 محاذاة شكلها أنضف
            newSheet.Range[newSheet.Cells[1, 1], newSheet.Cells[lastRow, lastCol]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            // 🟢 تحديد عمود Var (آخر عمود)
            var varRange = newSheet.Range[
                newSheet.Cells[2, colVar],
                newSheet.Cells[lastRow - 1, colVar]
            ];

            // ❌ مسح أي rules قديمة
            varRange.FormatConditions.Delete();

            // 🟢 Green (زيادة)
            var fcGreen = (Excel.FormatCondition)varRange.FormatConditions.Add(
                Excel.XlFormatConditionType.xlCellValue,
                Excel.XlFormatConditionOperator.xlGreater,
                "0"
            );

            fcGreen.Interior.Color = System.Drawing.ColorTranslator.ToOle(
                System.Drawing.Color.FromArgb(198, 239, 206) // Light Green
            );

            // 🔴 Red (نقصان)
            var fcRed = (Excel.FormatCondition)varRange.FormatConditions.Add(
                Excel.XlFormatConditionType.xlCellValue,
                Excel.XlFormatConditionOperator.xlLess,
                "0"
            );

            fcRed.Interior.Color = System.Drawing.ColorTranslator.ToOle(
                System.Drawing.Color.FromArgb(255, 199, 206) // Light Red
            );
            newSheet.Columns.AutoFit();
        }
        private bool SheetExists(Excel.Workbook wb, string name)
        {
            foreach (Excel.Worksheet ws in wb.Worksheets)
            {
                if (ws.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public Excel.Worksheet? FindSheetLike(Excel.Workbook wb, string pattern)
        {
            string p = pattern.Trim().ToLowerInvariant();
            foreach (Excel.Worksheet ws in wb.Worksheets)
            {
                if (ws.Name != null && ws.Name.ToLowerInvariant().Contains(p))
                    return ws;

                Marshal.ReleaseComObject(ws);
            }
            return null;
        }

        public Excel.Workbook? OpenReportHidden(Excel.Application app, string path)
        {
            var oldSec = app.AutomationSecurity;
            try
            {
                app.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable;
                app.ScreenUpdating = false;
                app.DisplayAlerts = false;
                app.EnableEvents = false;

                return app.Workbooks.Open(path, ReadOnly: true, UpdateLinks: 0);
            }
            catch
            {
                return null;
            }
            finally
            {
                app.EnableEvents = true;
                app.DisplayAlerts = true;
                app.ScreenUpdating = true;
                app.AutomationSecurity = oldSec;
            }
        }

        public int ColLetterToIndex(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            int res = 0;
            foreach (char ch in s)
            {
                if (ch < 'A' || ch > 'Z') continue;
                res = res * 26 + (ch - 'A' + 1);
            }
            return res;
        }

        public static string NormalizeName(string s)
        {
            s = (s ?? "").Trim();
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            return s.ToLowerInvariant();
        }

        // ====== VBA: NormalizeHeader + HeaderHasTokens ======
        private static string NormalizeHeader(string s)
        {
            s = (s ?? "").Trim().ToLowerInvariant();
            s = s.Replace("\t", "")
                 .Replace("-", "")
                 .Replace("–", "")
                 .Replace("_", "")
                 .Replace(" ", "")
                 .Replace("ـ", "")
                 .Replace(":", "")
                 .Replace("/", "")
                 .Replace("\\", "");
            return s;
        }

        private static bool HeaderHasTokens(string cellText, params string[] mustTokens)
        {
            string norm = NormalizeHeader(cellText);
            foreach (var tok in mustTokens)
            {
                if (!norm.Contains(NormalizeHeader(tok)))
                    return false;
            }
            return true;
        }

        // ====== VBA: FindHeaderRowAndCols (Today+Labor) and (Today+Count) ======
        public bool FindHeaderRowAndCols(
            Excel.Worksheet ws,
            out int headerRow,
            out int colTodayLabor,
            out int colTodayCount,
            out int leftHeaderCol,
            int scanTopRows = 30)
        {
            headerRow = 0;
            colTodayLabor = 0;
            colTodayCount = 0;
            leftHeaderCol = 0;

            int lastCol = ws.Cells[1, ws.Columns.Count].End[Excel.XlDirection.xlToLeft].Column;
            if (lastCol < 1) return false;

            for (int r = 1; r <= Math.Max(scanTopRows, 10); r++)
            {
                colTodayLabor = 0;
                colTodayCount = 0;
                leftHeaderCol = 0;

                for (int j = 1; j <= lastCol; j++)
                {
                    string txt = Convert.ToString(((Excel.Range)ws.Cells[r, j]).Value2) ?? "";
                    if (txt.Trim().Length == 0) continue;

                    if (leftHeaderCol == 0) leftHeaderCol = j;

                    if (HeaderHasTokens(txt, "today", "labor") || HeaderHasTokens(txt, "اليوم", "عماله") || HeaderHasTokens(txt, "اليوم", "عمالة"))
                        colTodayLabor = j;
                    else if (HeaderHasTokens(txt, "today", "count") || HeaderHasTokens(txt, "اليوم", "عدد"))
                        colTodayCount = j;
                }

                if (colTodayLabor > 0 && colTodayCount > 0)
                {
                    headerRow = r;
                    return true;
                }
            }

            return false;
        }

        // ====== VBA: QuickCheckPrevHasHeaders ======
        public bool QuickCheckPrevHasHeaders(Excel.Application app, string filePath)
        {
            Excel.Workbook? wb = null;
            try
            {
                wb = OpenReportHidden(app, filePath);
                if (wb == null) return false;

                var ws = FindSheetLike(wb, SubSheetPattern);
                if (ws == null) return false;

                bool ok = FindHeaderRowAndCols(ws, out _, out _, out _, out _, 15);
                Marshal.ReleaseComObject(ws);
                return ok;
            }
            finally
            {
                if (wb != null) wb.Close(false);
            }
        }

        // ====== VBA: ReadPrevDictFromRange ======
        public Dictionary<string, double> ReadPrevDictFromRange(
            Excel.Application app,
            string prevPath,
            string prevSheetName,
            out string prevTopLeftAddr,
            out double prevTotal)
        {
            var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            prevTopLeftAddr = "A1";
            prevTotal = 0;

            Excel.Workbook? wbPrev = null;
            Excel.Worksheet? wsPrev = null;

            try
            {
                wbPrev = OpenReportHidden(app, prevPath);
                if (wbPrev == null) return dict;

                wsPrev = wbPrev.Worksheets[prevSheetName] as Excel.Worksheet;
                if (wsPrev == null) return dict;

                if (!FindHeaderRowAndCols(wsPrev, out int hdrRow, out int cTL, out int cTC, out int cLeft, 30))
                    return dict;

                prevTopLeftAddr = ((Excel.Range)wsPrev.Cells[hdrRow, cLeft]).Address[false, false];

                int lastRow = wsPrev.Cells[wsPrev.Rows.Count, cTL].End[Excel.XlDirection.xlUp].Row;
                if (lastRow < hdrRow + 1) return dict;

                bool hasExplicitTotal = false;

                for (int r = hdrRow + 1; r <= lastRow; r++)
                {
                    string nm = (Convert.ToString(((Excel.Range)wsPrev.Cells[r, cTL]).Value2) ?? "").Trim();
                    if (nm.Length == 0) continue;

                    if (IsTotalRowName(nm))
                    {
                        prevTotal = ToDouble(((Excel.Range)wsPrev.Cells[r, cTC]).Value2);
                        hasExplicitTotal = true;
                        continue;
                    }

                    double q = ToDouble(((Excel.Range)wsPrev.Cells[r, cTC]).Value2);
                    string key = NormalizeName(nm);

                    if (!dict.ContainsKey(key))
                        dict.Add(key, q);
                    else
                        dict[key] += q;

                }

                if (!hasExplicitTotal)
                    prevTotal = dict.Values.Sum();

                return dict;
            }
            finally
            {
                if (wsPrev != null) Marshal.ReleaseComObject(wsPrev);
                if (wbPrev != null) wbPrev.Close(false);
            }
        }

        private static bool IsTotalRowName(string nm)
        {
            string s = (nm ?? "").Trim().ToLowerInvariant();
            return s is "total" or "totals" or "grand total"
                   or "اجمالى" or "الإجمالى" or "المجموع";
        }

        private static double ToDouble(object v)
        {
            if (v == null) return 0;
            if (v is double d) return d;
            if (double.TryParse(Convert.ToString(v), NumberStyles.Any, CultureInfo.InvariantCulture, out var x)) return x;
            if (double.TryParse(Convert.ToString(v), out x)) return x;
            return 0;
        }

        // ====== VBA: ReadTodayDictFromSelection_Filtered ======
        public Dictionary<string, double> ReadTodayFiltered(
     Excel.Worksheet wsCur,
     string nameColLetter, int nameStartRow,
     string countColLetter, int countStartRow,
     HashSet<string> selectedNormKeys)
        {
            var d = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            int lastRowName = wsCur.Cells[wsCur.Rows.Count, nameColLetter]
                .End[Excel.XlDirection.xlUp].Row;

            if (lastRowName < nameStartRow)
                return d;

            int offset = countStartRow - nameStartRow;

            for (int r = nameStartRow; r <= lastRowName; r++)
            {
                var nameCell = (Excel.Range)wsCur.Cells[r, nameColLetter];

                string nmRaw;

                if (nameCell.MergeCells)
                    nmRaw = (Convert.ToString(nameCell.MergeArea.Cells[1, 1].Value2) ?? "").Trim();
                else
                    nmRaw = (Convert.ToString(nameCell.Value2) ?? "").Trim();

                string nmKey = NormalizeName(nmRaw);

                if (nmKey.Length == 0) continue;
                if (!selectedNormKeys.Contains(nmKey)) continue;

                double value = 0;

                if (nameCell.MergeCells)
                {
                    var area = nameCell.MergeArea;

                    int startRow = area.Row;
                    int endRow = area.Row + area.Rows.Count - 1;

                    value = SumMergedBlock(wsCur, startRow, endRow, countColLetter, offset);

                    r = endRow; // ⛔ مهم
                }
                else
                {
                    int countRow = r + offset;
                    var cell = (Excel.Range)wsCur.Cells[countRow, countColLetter];
                    value = ToDouble(cell.Value2);
                }

                if (!d.ContainsKey(nmKey))
                    d.Add(nmKey, value);
                else
                    d[nmKey] += value;
            }

            return d;
        }

        public Dictionary<string, (double prev, double today)> MergePrevToday(
            Dictionary<string, double> prev,
            Dictionary<string, double> today)
        {
            var outd = new Dictionary<string, (double prev, double today)>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in prev)
                outd[kv.Key] = (kv.Value, 0);

            foreach (var kv in today)
            {
                if (outd.TryGetValue(kv.Key, out var a))
                    outd[kv.Key] = (a.prev, kv.Value);
                else
                    outd[kv.Key] = (0, kv.Value);
            }

            return outd;
        }

        public List<PreviewRow> BuildPreviewRows(
            Dictionary<string, (double prev, double today)> merged,
            double prevTotalExplicit)
        {
            var rows = new List<PreviewRow>();

            double sumPrev = 0, sumToday = 0;
            foreach (var kv in merged)
            {
                // skip total-like keys لو ظهرت
                if (IsTotalLikeName(kv.Key)) continue;

                sumPrev += kv.Value.prev;
                sumToday += kv.Value.today;

                rows.Add(new PreviewRow
                {
                    Name = kv.Key,
                    Previous = kv.Value.prev,
                    Today = kv.Value.today,
                    IsTotals = false
                });
            }

            double prevBase = prevTotalExplicit > 0 ? prevTotalExplicit : sumPrev;

            rows.Add(new PreviewRow
            {
                Name = "— Totals —",
                Previous = prevBase,
                Today = sumToday,
                IsTotals = true
            });

            return rows;
        }

        private static bool IsTotalLikeName(string nm)
        {
            string s = (nm ?? "").Trim().ToLowerInvariant();
            return s is "total" or "totals" or "grand total"
                   or "اجمالى" or "الإجمالى" or "المجموع";
        }

        // ====== ApplyAtColumnJ (نسخة عملية: Range mode + headers + var + totals) ======
        public void ApplyAtColumnJ(
            Excel.Worksheet wsCur,
            Dictionary<string, (double prev, double today)> merged,
            string prevTopLeftAddr)
        {
            if (merged.Count == 0) return;

            // header row = row of prevTopLeftAddr
            int headerRow = wsCur.Range[prevTopLeftAddr].Row;
            int colJ = 10;

            int colTL = colJ + 0;
            int colTC = colJ + 1;
            int colPL = colJ + 2;
            int colPC = colJ + 3;
            int colVar = colJ + 4;

            wsCur.Cells[headerRow, colTL].Value2 = "Today - Labor";
            wsCur.Cells[headerRow, colTC].Value2 = "Today - Count";
            wsCur.Cells[headerRow, colPL].Value2 = "Previous - Labor";
            wsCur.Cells[headerRow, colPC].Value2 = "Previous - Count";
            wsCur.Cells[headerRow, colVar].Value2 = "Var";

            int rOut = headerRow + 1;
            foreach (var kv in merged)
            {
                if (IsTotalLikeName(kv.Key)) continue;

                wsCur.Cells[rOut, colTL].Value2 = kv.Key;
                wsCur.Cells[rOut, colTC].Value2 = kv.Value.today;
                wsCur.Cells[rOut, colPL].Value2 = kv.Key;
                wsCur.Cells[rOut, colPC].Value2 = kv.Value.prev;
                wsCur.Cells[rOut, colVar].Value2 = kv.Value.today - kv.Value.prev;
                rOut++;
            }

            // Totals row
            wsCur.Cells[rOut, colTL].Value2 = "Totals";
            wsCur.Cells[rOut, colPL].Value2 = "Totals";

            var rngToday = wsCur.Range[wsCur.Cells[headerRow + 1, colTC], wsCur.Cells[rOut - 1, colTC]];
            var rngPrev = wsCur.Range[wsCur.Cells[headerRow + 1, colPC], wsCur.Cells[rOut - 1, colPC]];

            wsCur.Cells[rOut, colTC].Formula = "=SUM(" + rngToday.Address[false, false] + ")";
            wsCur.Cells[rOut, colPC].Formula = "=SUM(" + rngPrev.Address[false, false] + ")";
            wsCur.Cells[rOut, colVar].Formula = "=" +
                ((Excel.Range)wsCur.Cells[rOut, colTC]).Address[false, false] + "-" +
                ((Excel.Range)wsCur.Cells[rOut, colPC]).Address[false, false];

            Marshal.ReleaseComObject(rngToday);
            Marshal.ReleaseComObject(rngPrev);
        }

        public List<PrevFileItem> GetPrevFilesFromTodayFolder(Excel.Application app, Excel.Workbook activeWb)
        {
            var list = new List<PrevFileItem>();
            string basePath = activeWb.Path;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
                return list;

            var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".xlsm", ".xlsb" };
            foreach (var file in Directory.EnumerateFiles(basePath))
            {
                var fi = new FileInfo(file);
                if (fi.Name.StartsWith("~$")) continue;
                if (!exts.Contains(fi.Extension)) continue;
                if (string.Equals(fi.FullName, activeWb.FullName, StringComparison.OrdinalIgnoreCase)) continue;


                list.Add(new PrevFileItem
                {
                    DisplayName = fi.Name,
                    FullPath = fi.FullName
                });
                list = list
                .OrderByDescending(f => f.DisplayName.Contains("Report"))
                .ThenByDescending(f => new FileInfo(f.FullPath).LastWriteTime)
                .ToList();
            }
            return list;
        }
    }
}
