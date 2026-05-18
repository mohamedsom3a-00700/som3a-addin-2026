using Microsoft.Office.Interop.Excel;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SysActionInt = System.Action<int>;

namespace Som3a.Shared.Core
{
    public sealed class WorkbookCloneService
{
    private readonly Application _app;
    public WorkbookCloneService(Application app) => _app = app;

    public string SafeCloneActiveWorkbook(string saveAsPath, SysActionInt? progressPercent = null)
    {
        if (_app.ActiveWorkbook == null) throw new InvalidOperationException("No active workbook.");
        var src = _app.ActiveWorkbook;

        // 1) Create new workbook
        Workbook dst = _app.Workbooks.Add();
        try
        {
            _app.ScreenUpdating = false;
            progressPercent?.Invoke(2);

            // 2) Remove default sheets in dst (optional)
            while (dst.Worksheets.Count > 1)
            {
                Worksheet wsDel = (Worksheet)dst.Worksheets[1];
                wsDel.Delete();
                Marshal.ReleaseComObject(wsDel);
            }

            // 3) Copy all sheets
            int wsCount = src.Worksheets.Count;
            for (int i = 1; i <= wsCount; i++)
            {
                Worksheet srcWs = (Worksheet)src.Worksheets[i];
                srcWs.Copy(After: dst.Worksheets[dst.Worksheets.Count]); // copies entire sheet including formats/charts/shapes
                Marshal.ReleaseComObject(srcWs);

                int p = 2 + (int)(i * 35.0 / Math.Max(1, wsCount)); // up to ~37%
                progressPercent?.Invoke(p);
            }

            // Remove the first default sheet if still there and empty-name like "Sheet1"
            try
            {
                // لو عندك شيت افتراضي لسه موجود قبل النسخ
                // سيبه أو امسحه حسب رغبتك
            }
            catch { }

            progressPercent?.Invoke(40);

            // 4) Convert external formulas to values (in all dst sheets)
            ConvertExternalFormulasToValues(dst, p => progressPercent?.Invoke(40 + (int)(p * 30.0 / 100.0))); // 40..70

            // 5) Remove external names
            RemoveExternalNames(dst);

            progressPercent?.Invoke(80);

            // 6) Break all workbook links in dst (Phantom Links)
            BreakAllLinks(dst, p => progressPercent?.Invoke(80 + (int)(p * 15.0 / 100.0))); // 80..95

            // 7) Save
            EnsureDirectory(saveAsPath);
            dst.SaveAs(saveAsPath);
            progressPercent?.Invoke(100);

            return saveAsPath;
        }
        finally
        {
            _app.ScreenUpdating = true;
        }
    }

    private void ConvertExternalFormulasToValues(Workbook wb, SysActionInt? progressPercent = null)
    {
        int wsCount = wb.Worksheets.Count;
        int done = 0;

        foreach (Worksheet ws in wb.Worksheets)
        {
            done++;
            int p = (int)(done * 100.0 / Math.Max(1, wsCount));
            progressPercent?.Invoke(p);

            try
            {
                Range used = ws.UsedRange;
                if (used == null) continue;

                Range formulas = null;
                try { formulas = used.SpecialCells(XlCellType.xlCellTypeFormulas); }
                catch { formulas = null; }

                if (formulas == null) continue;

                foreach (Range c in formulas.Cells)
                {
                    try
                    {
                        string f = c.Formula?.ToString() ?? "";
                        if (f.IndexOf("[", StringComparison.Ordinal) >= 0)
                        {
                            // external ref -> freeze result
                            c.Value2 = c.Value2;
                        }
                    }
                    catch { }
                    finally
                    {
                        Marshal.ReleaseComObject(c);
                    }
                }

                Marshal.ReleaseComObject(formulas);
                Marshal.ReleaseComObject(used);
            }
            catch { }
            finally
            {
                Marshal.ReleaseComObject(ws);
            }
        }
    }

    private void RemoveExternalNames(Workbook wb)
    {
        try
        {
            int n = wb.Names.Count;
            // امشي بالعكس عشان الحذف
            for (int i = n; i >= 1; i--)
            {
                try
                {
                    Name nm = wb.Names.Item(i);
                    string refersTo = nm.RefersTo ?? "";
                    if (refersTo.Contains("["))
                    {
                        nm.Delete();
                    }
                    Marshal.ReleaseComObject(nm);
                }
                catch { }
            }
        }
        catch { }
    }

    private void BreakAllLinks(Workbook wb, SysActionInt? progressPercent = null)
    {
        try
        {
            var arr = wb.LinkSources(XlLink.xlExcelLinks) as Array;
            if (arr == null || arr.Length == 0) { progressPercent?.Invoke(100); return; }

            int total = arr.Length;
            int done = 0;

            foreach (var o in arr)
            {
                done++;
                int p = (int)(done * 100.0 / Math.Max(1, total));
                progressPercent?.Invoke(p);

                string link = o?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(link)) continue;

                try { wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks); }
                catch { }
            }

            progressPercent?.Invoke(100);
        }
        catch { progressPercent?.Invoke(100); }
    }

    private static void EnsureDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
}