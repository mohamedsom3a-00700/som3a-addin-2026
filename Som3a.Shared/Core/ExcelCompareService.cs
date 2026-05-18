using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a.Shared.Core
{
    public sealed class ExcelCompareService
    {
        private const string UpSheetName = "TASK_Up";
        private const string DownSheetName = "TASK_Down";

        public async Task<IReadOnlyList<CompareResultItem>> ProcessAsync(
            Excel.Workbook wb,
            string oldSheetName,
            string newSheetName,
            string idColLetter,
            string compareColLetter,
            string copyColLetter, // WBS column letter (optional) - READ FROM OLD
            int startRow,
            bool previewMode,
            IProgress<ProgressInfo> progress,
            CancellationToken token)
        {
            if (wb == null) throw new ArgumentNullException("wb");
            if (string.IsNullOrWhiteSpace(oldSheetName)) throw new ArgumentException("oldSheetName is required");
            if (string.IsNullOrWhiteSpace(newSheetName)) throw new ArgumentException("newSheetName is required");
            if (string.IsNullOrWhiteSpace(idColLetter)) throw new ArgumentException("idColLetter is required");
            if (string.IsNullOrWhiteSpace(compareColLetter)) throw new ArgumentException("compareColLetter is required");
            if (startRow < 1) throw new ArgumentOutOfRangeException("startRow");

            Excel.Worksheet wsOld = (Excel.Worksheet)wb.Worksheets[oldSheetName];
            Excel.Worksheet wsNew = (Excel.Worksheet)wb.Worksheets[newSheetName];

            int idCol = ColLetterToNum(idColLetter);
            int compCol = ColLetterToNum(compareColLetter);
            int extraCol = string.IsNullOrWhiteSpace(copyColLetter) ? 0 : ColLetterToNum(copyColLetter);

            List<CompareResultItem> results = new List<CompareResultItem>(256);

            Excel.Application app = wb.Application;
            bool prevScreenUpdating = app.ScreenUpdating;
            bool prevEnableEvents = app.EnableEvents;
            Excel.XlCalculation prevCalc = app.Calculation;

            Excel.Worksheet wsUp = null;
            Excel.Worksheet wsDown = null;

            try
            {
                app.ScreenUpdating = false;
                app.EnableEvents = false;
                app.Calculation = Excel.XlCalculation.xlCalculationManual;

                // ================= Load Old =================
                Dictionary<string, double> dictOld = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, object> dictExtra = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                int lastOld = LastRow(wsOld, idCol);
                for (int r = startRow; r <= lastOld; r++)
                {
                    token.ThrowIfCancellationRequested();

                    string id = TrimToString(wsOld.Cells[r, idCol].Value2);
                    if (string.IsNullOrWhiteSpace(id)) continue;

                    object oldObj = wsOld.Cells[r, compCol].Value2;
                    double oldVal;
                    if (TryToDouble(oldObj, out oldVal))
                    {
                        dictOld[id] = oldVal;

                        // WBS from OLD only
                        if (extraCol > 0)
                        {
                            object extraObj = wsOld.Cells[r, extraCol].Value2;
                            if (extraObj != null)
                                dictExtra[id] = extraObj;
                        }
                    }

                    // خفف الضغط على UI أثناء تحميل القديم
                    if (r % 200 == 0)
                        await Task.Delay(1);
                }

                // ================= Output Sheets =================
                int lastCol = LastCol(wsNew, 1);
                int pasteUp = 3;
                int pasteDown = 3;

                if (!previewMode)
                {
                    DeleteSheetIfExists(wb, UpSheetName);
                    DeleteSheetIfExists(wb, DownSheetName);

                    wsUp = (Excel.Worksheet)wb.Worksheets.Add();
                    wsUp.Name = UpSheetName;

                    wsDown = (Excel.Worksheet)wb.Worksheets.Add();
                    wsDown.Name = DownSheetName;

                    wsNew.Rows["1:2"].Copy(wsUp.Rows["1:2"]);
                    wsNew.Rows["1:2"].Copy(wsDown.Rows["1:2"]);
                }

                // ================= Main Loop =================
                int lastNew = LastRow(wsNew, idCol);
                int totalRows = Math.Max(0, lastNew - startRow + 1);
                int changes = 0;

                for (int r = startRow; r <= lastNew; r++)
                {
                    token.ThrowIfCancellationRequested();

                    string id = TrimToString(wsNew.Cells[r, idCol].Value2);

                    double oldVal2;
                    if (string.IsNullOrWhiteSpace(id) || !dictOld.TryGetValue(id, out oldVal2))
                    {
                        // Report أقل تكرارًا
                        if (r % 25 == 0) Report(progress, r, startRow, totalRows, changes);
                        if (r % 50 == 0) await Task.Delay(1);
                        continue;
                    }

                    object newObj = wsNew.Cells[r, compCol].Value2;
                    double newVal;
                    if (!TryToDouble(newObj, out newVal))
                    {
                        if (r % 25 == 0) Report(progress, r, startRow, totalRows, changes);
                        if (r % 50 == 0) await Task.Delay(1);
                        continue;
                    }

                    // WBS from OLD
                    string wbs = "";
                    if (extraCol > 0)
                    {
                        object extraObj2;
                        if (dictExtra.TryGetValue(id, out extraObj2))
                            wbs = TrimToString(extraObj2);
                    }

                    // ===== compare + copy =====
                    if (newVal > oldVal2)
                    {
                        results.Add(new CompareResultItem
                        {
                            TaskId = id,
                            OldValue = oldVal2,
                            NewValue = newVal,
                            ChangeType = "Increase",
                            Status = UpSheetName,
                            WbsId = wbs
                        });
                        changes++;

                        if (!previewMode && wsUp != null)
                        {
                            Excel.Range rowRange = wsNew.Range[wsNew.Cells[r, 1], wsNew.Cells[r, lastCol]];
                            rowRange.Copy(wsUp.Cells[pasteUp, 1]);

                            if (extraCol > 0)
                                wsUp.Cells[pasteUp, extraCol].Value2 = wbs;

                            wsUp.Rows[pasteUp].Interior.Color = ToOleColor(198, 239, 206);
                            pasteUp++;
                        }
                    }
                    else if (newVal < oldVal2)
                    {
                        results.Add(new CompareResultItem
                        {
                            TaskId = id,
                            OldValue = oldVal2,
                            NewValue = newVal,
                            ChangeType = "Decrease",
                            Status = DownSheetName,
                            WbsId = wbs
                        });
                        changes++;

                        if (!previewMode && wsDown != null)
                        {
                            Excel.Range rowRange = wsNew.Range[wsNew.Cells[r, 1], wsNew.Cells[r, lastCol]];
                            rowRange.Copy(wsDown.Cells[pasteDown, 1]);

                            if (extraCol > 0)
                                wsDown.Cells[pasteDown, extraCol].Value2 = wbs;

                            wsDown.Rows[pasteDown].Interior.Color = ToOleColor(255, 199, 206);
                            pasteDown++;
                        }
                    }

                    // Report بعد النسخ عشان % يبقى واقعي
                    if (r % 10 == 0 || r == lastNew)
                        Report(progress, r, startRow, totalRows, changes);

                    // خليك لطيف مع الـ UI كل شوية
                    if (r % 40 == 0)
                        await Task.Delay(1);
                }

                // report final 100%
                if (totalRows > 0)
                    progress.Report(new ProgressInfo { Percent = 100, RowsDone = totalRows, Changes = changes });

                return results;
            }
            finally
            {
                try
                {
                    app.ScreenUpdating = prevScreenUpdating;
                    app.EnableEvents = prevEnableEvents;
                    app.Calculation = prevCalc;
                }
                catch { }

                SafeRelease(wsUp);
                SafeRelease(wsDown);
                SafeRelease(wsOld);
                SafeRelease(wsNew);
            }
        }

        // ===== Progress Struct =====
        public struct ProgressInfo
        {
            public int Percent;
            public int RowsDone;
            public int Changes;
        }

        private static void Report(IProgress<ProgressInfo> progress, int r, int startRow, int totalRows, int changes)
        {
            if (progress == null) return;

            int rowsDone = r - startRow + 1;
            if (rowsDone < 0) rowsDone = 0;
            if (rowsDone > totalRows) rowsDone = totalRows;

            int pct = totalRows == 0 ? 0 : (int)Math.Round((rowsDone * 100.0) / totalRows);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;

            progress.Report(new ProgressInfo { Percent = pct, RowsDone = rowsDone, Changes = changes });
        }

        private static int ToOleColor(int r, int g, int b)
        {
            // Excel OLE_COLOR is BGR
            return (b << 16) | (g << 8) | r;
        }

        private static int ColLetterToNum(string colLetter)
        {
            colLetter = colLetter.Trim().ToUpperInvariant();
            int sum = 0;
            for (int i = 0; i < colLetter.Length; i++)
            {
                char c = colLetter[i];
                if (c < 'A' || c > 'Z') throw new ArgumentException("Invalid column letter");
                sum = sum * 26 + (c - 'A' + 1);
            }
            return sum;
        }

        private static int LastRow(Excel.Worksheet ws, int colNum)
        {
            Excel.Range last = ws.Cells[ws.Rows.Count, colNum].End[Excel.XlDirection.xlUp];
            return last.Row;
        }

        private static int LastCol(Excel.Worksheet ws, int headerRow)
        {
            Excel.Range last = ws.Cells[headerRow, ws.Columns.Count].End[Excel.XlDirection.xlToLeft];
            return last.Column;
        }

        private static string TrimToString(object v)
        {
            if (v == null) return "";
            string s = Convert.ToString(v, CultureInfo.InvariantCulture);
            return s == null ? "" : s.Trim();
        }

        private static bool TryToDouble(object v, out double d)
        {
            if (v == null) { d = 0; return false; }
            if (v is double) { d = (double)v; return true; }

            string s = Convert.ToString(v, CultureInfo.InvariantCulture);
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return true;

            d = 0;
            return false;
        }

        private static void DeleteSheetIfExists(Excel.Workbook wb, string name)
        {
            Excel.Worksheet ws = null;
            try
            {
                ws = (Excel.Worksheet)wb.Worksheets[name];
                bool prevAlerts = wb.Application.DisplayAlerts;
                wb.Application.DisplayAlerts = false;
                ws.Delete();
                wb.Application.DisplayAlerts = prevAlerts;
            }
            catch { }
            finally
            {
                SafeRelease(ws);
            }
        }

        private static void SafeRelease(object comObj)
        {
            try
            {
                if (comObj != null && Marshal.IsComObject(comObj))
                    Marshal.ReleaseComObject(comObj);
            }
            catch { }
        }
    }
}
