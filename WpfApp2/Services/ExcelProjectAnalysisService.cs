using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Excel;
using Som3a.Shared.Models;
using Som3a.Shared.Utils;

namespace Som3a_WPF_UI.Services
{
    public sealed class ExcelProjectAnalysisService
    {
        private readonly Application _app;

        public ExcelProjectAnalysisService(object excelApp)
        {
            _app = (Application)excelApp;
        }

        public List<string> GetWorksheetNames()
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var list = new List<string>();
            foreach (Worksheet ws in wb.Worksheets)
                list.Add(ws.Name);
            return list;
        }

        // ==== VBA: AnalyzeAndAppend (counts distinct values in a header column) ====
        public Dictionary<string, int> AnalyzeColumnCounts(string sheetName, string headerName, int startRow)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var ws = (Worksheet)wb.Worksheets[sheetName];

            int lastRow = ws.Cells[ws.Rows.Count, 1].End[XlDirection.xlUp].Row;
            int lastCol = ws.Cells[1, ws.Columns.Count].End[XlDirection.xlToLeft].Column;

            int targetCol = 0;
            for (int c = 1; c <= lastCol; c++)
            {
                var h = Convert.ToString(ws.Cells[1, c].Value2) ?? "";
                if (string.Equals(h.Trim(), headerName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    targetCol = c;
                    break;
                }
            }
            if (targetCol == 0)
                throw new InvalidOperationException($"Column '{headerName}' not found on row 1 in sheet: {sheetName}");

            if (startRow < 2) startRow = 2;
            if (lastRow < startRow) return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            Range rng = ws.Range[ws.Cells[startRow, targetCol], ws.Cells[lastRow, targetCol]];
            object[,] arr = (object[,])rng.Value2;

            int rows = arr.GetLength(0);
            for (int r = 1; r <= rows; r++)
            {
                var v = (arr[r, 1] == null) ? "" : Convert.ToString(arr[r, 1])!.Trim();
                if (string.IsNullOrEmpty(v)) continue;

                if (dict.TryGetValue(v, out var n))
                    dict[v] = n + 1;
                else
                    dict[v] = 1;

            }

            return dict;
        }

        // ==== Validation (بديل PerformValidation + listchk_sheets) ====
        public (bool IsOk, List<SheetCheckRow> Rows) ValidateRequiredHeaders(
            string activitiesSheet, string logicSheet, string resourcesSheet)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var wsA = (Worksheet)wb.Worksheets[activitiesSheet];
            var wsL = (Worksheet)wb.Worksheets[logicSheet];
            var wsR = (Worksheet)wb.Worksheets[resourcesSheet];

            var rows = new List<SheetCheckRow>();

            // من الـ VBA (مذكور صراحة في CreateInProgressSplit)
            // Activities required:
            var reqA = new[]
            {
                "status_code","task_code","act_start_date","act_end_date","delete_record_flag",
                "remain_drtn_hr_cnt","target_drtn_hr_cnt","act_drtn_hr_cnt","complete_pct"
            };

            // Relationships required (من CreateRelationshipsSplitAndGenerateFS signature في VBA)
            var reqL = new[]
            {
                "pred_task_id","task_id","pred_type","delete_record_flag",
                "PREDTASK__status_code","TASK__status_code"
            };

            // Resources required (من CreateResourcesUp_WithSplitAB signature في VBA)
            var reqR = new[]
            {
                "task_id","delete_record_flag"
                // والباقي يتحدد حسب منطقك في resources split
            };

            bool ok = true;

            ok &= CheckHeaders(wsA, activitiesSheet, reqA, rows);
            ok &= CheckHeaders(wsL, logicSheet, reqL, rows);
            ok &= CheckHeaders(wsR, resourcesSheet, reqR, rows);

            return (ok, rows);
        }

        private bool CheckHeaders(Worksheet ws, string section, string[] required, List<SheetCheckRow> outRows)
        {
            bool ok = true;
            foreach (var h in required)
            {
                int col = FindHeaderColumn(ws, h, 1);
                bool found = col > 0;
                if (!found) ok = false;

                outRows.Add(new SheetCheckRow
                {
                    Section = section,
                    RequiredColumn = h,
                    HeaderFound = found ? "Yes" : "No",
                    Location = found ? $"Row 1 Col {col}" : "-"
                });
            }
            return ok;
        }

        // ==== VBA helper ====
        private int FindHeaderColumn(Worksheet ws, string header, int headerRow)
        {
            int lastCol = ws.Cells[headerRow, ws.Columns.Count].End[XlDirection.xlToLeft].Column;
            for (int c = 1; c <= lastCol; c++)
            {
                var v = Convert.ToString(ws.Cells[headerRow, c].Value2) ?? "";
                if (string.Equals(v.Trim(), header.Trim(), StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return 0;
        }

        private bool WorksheetExists(string name)
        {
            var wb = _app.ActiveWorkbook;
            if (wb == null) return false;
            foreach (Worksheet ws in wb.Worksheets)
                if (string.Equals(ws.Name, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private Worksheet GetOrCreateSheet(string name, bool clear)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            Worksheet ws;
            if (WorksheetExists(name))
            {
                ws = (Worksheet)wb.Worksheets[name];
                if (clear) ws.Cells.Clear();
                return ws;
            }

            ws = (Worksheet)wb.Worksheets.Add(After: wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = name;
            return ws;
        }

        // ============================================================
        // VBA: CreateInProgressSplit_KeepOriginal
        // ============================================================
        public void CreateInProgressSplit_KeepOriginal(
            string activitiesSheetName,
            int startRow,
            string suffix,
            DateTime finishDate)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var wsSrc = (Worksheet)wb.Worksheets[activitiesSheetName];

            int lastRow = wsSrc.Cells[wsSrc.Rows.Count, 1].End[XlDirection.xlUp].Row;
            int lastCol = wsSrc.Cells[1, wsSrc.Columns.Count].End[XlDirection.xlToLeft].Column;
            if (lastRow < startRow) throw new InvalidOperationException("No data after startRow in Activities sheet.");

            // required cols
            int cStatus = FindHeaderColumn(wsSrc, "status_code", 1);
            int cTaskCode = FindHeaderColumn(wsSrc, "task_code", 1);
            int cStart = FindHeaderColumn(wsSrc, "act_start_date", 1);
            int cEnd = FindHeaderColumn(wsSrc, "act_end_date", 1);
            int cDel = FindHeaderColumn(wsSrc, "delete_record_flag", 1);
            int cRemain = FindHeaderColumn(wsSrc, "remain_drtn_hr_cnt", 1);
            int cTarget = FindHeaderColumn(wsSrc, "target_drtn_hr_cnt", 1);
            int cActDrtn = FindHeaderColumn(wsSrc, "act_drtn_hr_cnt", 1);
            int cPct = FindHeaderColumn(wsSrc, "complete_pct", 1);

            if (new[] { cStatus, cTaskCode, cStart, cEnd, cDel, cRemain, cTarget, cActDrtn, cPct }.Any(x => x == 0))
                throw new InvalidOperationException("Missing required columns in Activities sheet.");

            string newName = activitiesSheetName + suffix;
            var wsNew = GetOrCreateSheet(newName, clear: true);

            // Row 1 headers copy
            wsNew.Cells[1, 1].Resize[1, lastCol].Value2 = wsSrc.Cells[1, 1].Resize[1, lastCol].Value2;
            // Row 2 copy (VBA explicitly copies row2)
            wsNew.Cells[2, 1].Resize[1, lastCol].Value2 = wsSrc.Cells[2, 1].Resize[1, lastCol].Value2;

            int writeRow = 3;

            // Scan in-progress rows in source
            for (int r = startRow; r <= lastRow; r++)
            {
                var status = Convert.ToString(wsSrc.Cells[r, cStatus].Value2) ?? "";
                if (!string.Equals(status.Trim(), "In Progress", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(status.Trim(), "in progress", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Block1: original (mark delete_record_flag = "d")
                CopyRow(wsSrc, r, wsNew, writeRow, lastCol);
                wsNew.Cells[writeRow, cDel].Value2 = "d";
                writeRow++;

                // Block2: Not started clone (task_code += "-B", clear dates, target=remain, pct=0)
                CopyRow(wsSrc, r, wsNew, writeRow, lastCol);
                wsNew.Cells[writeRow, cTaskCode].Value2 = (Convert.ToString(wsSrc.Cells[r, cTaskCode].Value2) ?? "") + "-B";
                wsNew.Cells[writeRow, cStart].Value2 = null;
                wsNew.Cells[writeRow, cEnd].Value2 = null;
                wsNew.Cells[writeRow, cTarget].Value2 = wsSrc.Cells[r, cRemain].Value2;
                wsNew.Cells[writeRow, cPct].Value2 = 0;
                writeRow++;

                // Block3: completed clone (task_code += "-A", end=finishDate, remain=0, target=act_drtn, pct=100)
                CopyRow(wsSrc, r, wsNew, writeRow, lastCol);
                wsNew.Cells[writeRow, cTaskCode].Value2 = (Convert.ToString(wsSrc.Cells[r, cTaskCode].Value2) ?? "") + "-A";
                wsNew.Cells[writeRow, cEnd].Value2 = finishDate;
                wsNew.Cells[writeRow, cRemain].Value2 = 0;
                wsNew.Cells[writeRow, cTarget].Value2 = wsSrc.Cells[r, cActDrtn].Value2;
                wsNew.Cells[writeRow, cPct].Value2 = 100;
                writeRow++;
            }
        }

        private void CopyRow(Worksheet wsSrc, int srcRow, Worksheet wsDst, int dstRow, int lastCol)
        {
            wsDst.Cells[dstRow, 1].Resize[1, lastCol].Value2 = wsSrc.Cells[srcRow, 1].Resize[1, lastCol].Value2;
        }

        // ============================================================
        // VBA: CreateRelationshipsSplitAndGenerateFS
        // (هنا “core skeleton” مطابق: copy + generate FS links + uniqueness via TripleKey)
        // ============================================================
        public void CreateRelationshipsSplitAndGenerateFS(
            string relationshipsSheetName,
            string activitiesSourceSheetName,
            string activitiesNewSheetName,
            int startRow)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");

            var wsRel = (Worksheet)wb.Worksheets[relationshipsSheetName];
            var wsActNew = (Worksheet)wb.Worksheets[activitiesNewSheetName];

            // headers in relationships
            int cPred = FindHeaderColumn(wsRel, "pred_task_id", 1);
            int cTask = FindHeaderColumn(wsRel, "task_id", 1);
            int cType = FindHeaderColumn(wsRel, "pred_type", 1);
            int cDel = FindHeaderColumn(wsRel, "delete_record_flag", 1);
            if (new[] { cPred, cTask, cType, cDel }.Any(x => x == 0))
                throw new InvalidOperationException("Missing required columns in Relationships sheet.");

            int lastRow = wsRel.Cells[wsRel.Rows.Count, 1].End[XlDirection.xlUp].Row;
            int lastCol = wsRel.Cells[1, wsRel.Columns.Count].End[XlDirection.xlToLeft].Column;

            string newName = relationshipsSheetName + " up";
            var wsNew = GetOrCreateSheet(newName, clear: true);

            wsNew.Cells[1, 1].Resize[1, lastCol].Value2 = wsRel.Cells[1, 1].Resize[1, lastCol].Value2;
            wsNew.Cells[2, 1].Resize[1, lastCol].Value2 = wsRel.Cells[2, 1].Resize[1, lastCol].Value2;

            int writeRow = 3;

            // copy original relationships (with delete flag adjustments لو موجودة عندك)
            for (int r = startRow; r <= lastRow; r++)
            {
                CopyRow(wsRel, r, wsNew, writeRow, lastCol);
                writeRow++;
            }

            // generate FS for A/B tasks (concept from VBA: create FS between -B and -A etc.)
            // هنا بنحافظ على uniqueness باستخدام BuildTripleKey
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int newLastRow = wsNew.Cells[wsNew.Rows.Count, 1].End[XlDirection.xlUp].Row;
            for (int r = 3; r <= newLastRow; r++)
            {
                var pred = Convert.ToString(wsNew.Cells[r, cPred].Value2) ?? "";
                var succ = Convert.ToString(wsNew.Cells[r, cTask].Value2) ?? "";
                var typ = Convert.ToString(wsNew.Cells[r, cType].Value2) ?? "";
                existing.Add(RelUtils.BuildTripleKey(pred, succ, typ));
            }

            // مثال توليد FS: لو succ endswith "-A" اربط pred "-B" → succ "-A" (ده مجرد skeleton)
            // انت عندك منطق أدق داخل VBA (passes)، تقدر تحطه هنا بنفس الفكرة.
            var genPairs = new List<(string Pred, string Succ, string Type)>();

            // TODO: implement full passes like VBA (Pass1..Pass4) using activitiesNew sheet
            // for now: leave placeholder

            foreach (var p in genPairs)
            {
                var k = RelUtils.BuildTripleKey(p.Pred, p.Succ, p.Type);
                if (existing.Contains(k)) continue;

                wsNew.Cells[writeRow, cPred].Value2 = p.Pred;
                wsNew.Cells[writeRow, cTask].Value2 = p.Succ;
                wsNew.Cells[writeRow, cType].Value2 = p.Type; // "FS"
                wsNew.Cells[writeRow, cDel].Value2 = "";      // or "a" depending on VBA
                writeRow++;
                existing.Add(k);
            }
        }

        // ============================================================
        // VBA: CreateResourcesUp_WithSplitAB_UF
        // ============================================================
        public void CreateResourcesUp_WithSplitAB(
            string resourcesSheetName,
            string activitiesSheetName,
            string activitiesUpSheetName,
            int startRow,
            string suffix,
            bool clearExisting,
            bool copyColWidths)
        {
            var wb = _app.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var wsRes = (Worksheet)wb.Worksheets[resourcesSheetName];

            int lastRow = wsRes.Cells[wsRes.Rows.Count, 1].End[XlDirection.xlUp].Row;
            int lastCol = wsRes.Cells[1, wsRes.Columns.Count].End[XlDirection.xlToLeft].Column;

            int cTaskId = FindHeaderColumn(wsRes, "task_id", 1);
            int cDel = FindHeaderColumn(wsRes, "delete_record_flag", 1);
            if (cTaskId == 0 || cDel == 0)
                throw new InvalidOperationException("Missing required columns in Resources sheet.");

            string newName = resourcesSheetName + suffix;
            var wsNew = GetOrCreateSheet(newName, clear: clearExisting);

            wsNew.Cells[1, 1].Resize[1, lastCol].Value2 = wsRes.Cells[1, 1].Resize[1, lastCol].Value2;
            wsNew.Cells[2, 1].Resize[1, lastCol].Value2 = wsRes.Cells[2, 1].Resize[1, lastCol].Value2;

            int writeRow = 3;

            // Core idea: copy rows + duplicate for -A / -B where needed (split logic)
            // في VBA عندك logic تفصيلي لعمل split AB حسب TASK sheet/task_code mapping.
            // هنا skeleton: لو task_id endswith "-A" أو "-B" هنمرره، وإلا نعمل نسخه A+B حسب mapping من activitiesUp.
            for (int r = startRow; r <= lastRow; r++)
            {
                CopyRow(wsRes, r, wsNew, writeRow, lastCol);
                writeRow++;
            }

            if (copyColWidths)
            {
                for (int c = 1; c <= lastCol; c++)
                    wsNew.Columns[c].ColumnWidth = wsRes.Columns[c].ColumnWidth;
            }
        }
    }
}
