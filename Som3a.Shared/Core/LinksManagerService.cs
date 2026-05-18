using Microsoft.Office.Interop.Excel;
using Som3a.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SysAction = System.Action;
using SysActionInt = System.Action<int>;

namespace Som3a.Shared.Core
{
    public sealed class LinksManagerService
    {
        private readonly Application _app;

        public LinksManagerService(Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public List<WorkbookItem> GetOpenWorkbooks()
        {
            var list = new List<WorkbookItem>();

            foreach (Workbook wb in _app.Workbooks)
            {
                // في VBA كان بيستبعد ThisWorkbook. في VSTO مش عندنا ThisWorkbook بشكل مباشر.
                // هنضيف كل المفتوحين، وإنت لو عايز تستبعد Workbook معين لاحقًا نعملها بسهولة.
                list.Add(new WorkbookItem(wb.Name));
            }

            return list;
        }

        public Workbook? TryGetWorkbookByName(string workbookName)
        {
            if (string.IsNullOrWhiteSpace(workbookName)) return null;
            try { return _app.Workbooks[workbookName]; }
            catch { return null; }
        }

        private static bool TryGetShapeText(Microsoft.Office.Interop.Excel.Shape shp, out string text)
        {
            text = "";

            // 1) TextFrame2 (الأفضل)
            try
            {
                var tf2 = shp.TextFrame2;
                if (tf2 != null && tf2.HasText == Microsoft.Office.Core.MsoTriState.msoTrue)
                {
                    text = tf2.TextRange.Text ?? "";
                    return !string.IsNullOrWhiteSpace(text);
                }
            }
            catch { }

            // 2) fallback TextFrame (بدون HasText)
            try
            {
                var tf = shp.TextFrame; // Microsoft.Office.Interop.Excel.TextFrame
                if (tf != null)
                {
                    // بعض الإصدارات لا تملك HasText -> اقرأ النص مباشرة
                    string t = tf.Characters().Text?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        text = t;
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }


        public List<LinkTypeItem> LoadAllLinkTypes()
        {
            return new List<LinkTypeItem>
            {
                new LinkTypeItem("LinkSources"),
                new LinkTypeItem("Formulas"),
                new LinkTypeItem("NameManager"),
                new LinkTypeItem("ChartSeries"),
                new LinkTypeItem("ShapeText"),
                new LinkTypeItem("Connections"),

            };
        }
        public System.Threading.Tasks.Task<int> BreakAllLinkSourcesAsync(
    Workbook wb,
    SysActionInt? onProgress = null)
        {
            return StaTaskRunner.Run(() =>
            {
                int broken = 0;

                var arr = wb.LinkSources(XlLink.xlExcelLinks) as Array;
                if (arr == null || arr.Length == 0)
                {
                    onProgress?.Invoke(100);
                    return 0;
                }

                int total = arr.Length;
                int done = 0;

                _app.ScreenUpdating = false;
                try
                {
                    foreach (var o in arr)
                    {
                        done++;
                        onProgress?.Invoke((int)(done * 100.0 / Math.Max(1, total)));

                        var link = o?.ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(link)) continue;

                        try
                        {
                            wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks);
                            broken++;
                        }
                        catch { }
                    }
                }
                finally
                {
                    _app.ScreenUpdating = true;
                    onProgress?.Invoke(100);
                }

                return broken;
            });
        }

        public List<LinkTypeItem> LoadTypesBasedOnWorkbook(Workbook wb, SysActionInt? progressPercent = null)
        {
            bool hasLinkSources = false, hasFormulas = false, hasNames = false, hasCharts = false, hasShapes = false;

            // 1) LinkSources
            try
            {
                var arrLinks = wb.LinkSources(XlLink.xlExcelLinks);
                if (arrLinks is Array a && a.Length > 0) hasLinkSources = true;
            }
            catch { }

            int wsCount = wb.Worksheets.Count;
            int done = 0;

            // 2) Formulas
            if (!hasFormulas)
            {
                foreach (Worksheet ws in wb.Worksheets)
                {
                    done++;
                    progressPercent?.Invoke((int)(done * 100.0 / Math.Max(1, wsCount)));

                    try
                    {
                        if (ws.UsedRange != null && ws.UsedRange.CountLarge > 1)
                        {
                            Range? formulas = null;
                            try { formulas = ws.UsedRange.SpecialCells(XlCellType.xlCellTypeFormulas); }
                            catch { formulas = null; }

                            if (formulas != null)
                            {
                                foreach (Range c in formulas.Cells)
                                {
                                    try
                                    {
                                        var f = c.Formula?.ToString() ?? "";
                                        if (f.Contains("["))
                                        {
                                            hasFormulas = true;
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }

                    if (hasFormulas) break;
                }
            }

            // 3) NameManager
            if (!hasNames)
            {
                try
                {
                    int n = wb.Names.Count;
                    for (int i = 1; i <= n; i++)
                    {
                        Name nm;
                        try { nm = wb.Names.Item(i); }
                        catch { continue; }

                        string refersTo = "";
                        try { refersTo = nm.RefersTo ?? ""; } catch { }

                        if (refersTo.Contains("["))
                        {
                            hasNames = true;
                            break;
                        }

                        if (i % 50 == 0) { /* يفضل DoEvents في VBA - هنا مش محتاج */ }
                    }
                }
                catch { }
            }

            // 4) Charts
            if (!hasCharts)
            {
                try
                {
                    foreach (Worksheet ws in wb.Worksheets)
                    {
                        foreach (ChartObject co in ws.ChartObjects())
                        {
                            try
                            {
                                var sc = co.Chart.SeriesCollection();
                                if (sc != null && sc.Count > 0)
                                {
                                    Series s1 = sc.Item(1);
                                    var sf = s1.Formula?.ToString() ?? "";
                                    if (sf.Contains("["))
                                    {
                                        hasCharts = true;
                                        break;
                                    }
                                }
                            }
                            catch { }
                            if (hasCharts) break;
                        }
                        if (hasCharts) break;
                    }
                }
                catch { }
            }

            // 5) Shapes (فقط اكتشاف وجود External links في نصوص الـ Shapes)
            if (!hasShapes)
            {
                try
                {
                    foreach (Worksheet ws in wb.Worksheets)
                    {
                        foreach (Shape shp in ws.Shapes)
                        {
                            try
                            {
                                if (TryGetShapeText(shp, out string t) && t.Contains("["))
                                {
                                    // مجرد وجود [] يكفي نعتبر عندنا ShapeText
                                    hasShapes = true;
                                    break;
                                }
                            }
                            catch { }
                        }

                        if (hasShapes) break;
                    }
                }
                catch { }
            }


            var res = new List<LinkTypeItem>();
            if (hasLinkSources) res.Add(new LinkTypeItem("LinkSources"));
            if (hasFormulas) res.Add(new LinkTypeItem("Formulas"));
            if (hasNames) res.Add(new LinkTypeItem("NameManager"));
            if (hasCharts) res.Add(new LinkTypeItem("ChartSeries"));
            if (hasShapes) res.Add(new LinkTypeItem("ShapeText"));

            return res;
        }

        public List<LinkItem> LoadLinksByType(Workbook wb, string linkType, SysActionInt? progressPercent = null)
        {
            var list = new List<LinkItem>();
            int wsCount = wb.Worksheets.Count;

            // تقدير progress مشابه لفكرة VBA
            int totalSteps = Math.Max(1, wsCount);
            int step = 0;

            // 1) LinkSources
            if (linkType == "LinkSources")
            {
                try
                {
                    var arrLinks = wb.LinkSources(XlLink.xlExcelLinks);
                    if (arrLinks is Array a)
                    {
                        foreach (var o in a)
                        {
                            string link = o?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(link)) continue;

                            list.Add(new LinkItem
                            {
                                LinkSource = link,
                                SheetOrObject = "-",
                                CellOrDetail = "-",
                                Type = "LinkSources",
                                TagLink = link
                            });
                        }
                    }
                }
                catch { }

                progressPercent?.Invoke(100);
                return list;
            }

            // 2) Formulas
            if (linkType == "Formulas")
            {
                foreach (Worksheet ws in wb.Worksheets)
                {
                    step++;
                    progressPercent?.Invoke((int)(step * 100.0 / totalSteps));

                    try
                    {
                        Range? formulas = null;
                        try { formulas = ws.UsedRange.SpecialCells(XlCellType.xlCellTypeFormulas); }
                        catch { formulas = null; }

                        if (formulas == null) continue;

                        foreach (Range c in formulas.Cells)
                        {
                            try
                            {
                                string f = c.Formula?.ToString() ?? "";
                                if (!f.Contains("[")) continue;

                                string link = ExcelLinkTextHelper.GetExternalLinkFromText(f);
                                if (string.IsNullOrWhiteSpace(link)) continue;

                                list.Add(new LinkItem
                                {
                                    LinkSource = link,
                                    SheetOrObject = ws.Name,
                                    CellOrDetail = c.Address,
                                    Type = "Formula",
                                    TagLink = link
                                });
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                progressPercent?.Invoke(100);
                return list;
            }

            // 3) NameManager
            if (linkType == "NameManager")
            {
                try
                {
                    int n = wb.Names.Count;
                    // زي VBA: نحول refersTo لـ array عشان نقلل ضغط COM
                    var arr = new string[n + 1];

                    for (int i = 1; i <= n; i++)
                    {
                        try { arr[i] = wb.Names.Item(i).RefersTo ?? ""; }
                        catch { arr[i] = ""; }
                    }

                    for (int i = 1; i <= n; i++)
                    {
                        string refersTo = arr[i] ?? "";
                        if (string.IsNullOrWhiteSpace(refersTo)) continue;
                        if (!refersTo.Contains("[")) continue;

                        string link = ExcelLinkTextHelper.GetExternalLinkFromText(refersTo);
                        if (string.IsNullOrWhiteSpace(link)) continue;

                        string name = "";
                        try { name = wb.Names.Item(i).Name; } catch { name = ""; }

                        list.Add(new LinkItem
                        {
                            LinkSource = link,
                            SheetOrObject = "Name: " + name,
                            CellOrDetail = "-",
                            Type = "NameManager",
                            TagLink = link
                        });

                        // progress تقريبًا كل 50 زي VBA
                        if (i % 50 == 0)
                        {
                            int p = (int)(i * 100.0 / Math.Max(1, n));
                            progressPercent?.Invoke(p);
                        }
                    }
                }
                catch { }

                progressPercent?.Invoke(100);
                return list;
            }

            // 4) ChartSeries
            if (linkType == "ChartSeries")
            {
                foreach (Worksheet ws in wb.Worksheets)
                {
                    step++;
                    progressPercent?.Invoke((int)(step * 100.0 / totalSteps));

                    try
                    {
                        foreach (ChartObject co in ws.ChartObjects())
                        {
                            SeriesCollection sc;
                            try { sc = co.Chart.SeriesCollection(); }
                            catch { continue; }

                            int scCount = 0;
                            try { scCount = sc.Count; } catch { scCount = 0; }

                            for (int i = 1; i <= scCount; i++)
                            {
                                try
                                {
                                    Series ser = sc.Item(i);
                                    string sf = ser.Formula?.ToString() ?? "";
                                    if (!sf.Contains("[")) continue;

                                    string link = ExcelLinkTextHelper.GetExternalLinkFromText(sf);
                                    if (string.IsNullOrWhiteSpace(link)) continue;

                                    list.Add(new LinkItem
                                    {
                                        LinkSource = link,
                                        SheetOrObject = ws.Name,
                                        CellOrDetail = "Chart: " + co.Name,
                                        Type = "ChartSeries",
                                        TagLink = link
                                    });
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }

                progressPercent?.Invoke(100);
                return list;
            }

            // 5) ShapeText
            if (linkType == "ShapeText")
            {
                foreach (Worksheet ws in wb.Worksheets)
                {
                    step++;
                    progressPercent?.Invoke((int)(step * 100.0 / totalSteps));

                    try
                    {
                        foreach (Shape shp in ws.Shapes)
                        {
                            try
                            {
                                if (TryGetShapeText(shp, out string t) && t.Contains("["))
                                {
                                    string link = ExcelLinkTextHelper.GetExternalLinkFromText(t);
                                    if (!string.IsNullOrWhiteSpace(link))
                                    {
                                        list.Add(new LinkItem
                                        {
                                            LinkSource = link,
                                            SheetOrObject = ws.Name,
                                            CellOrDetail = "Shape: " + shp.Name,
                                            Type = "ShapeText",
                                            TagLink = link
                                        });
                                    }
                                }

                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                progressPercent?.Invoke(100);
                return list;
            }
            if (linkType == "Connections")
            {
                try
                {
                    foreach (WorkbookConnection cn in wb.Connections)
                    {
                        string src = "";

                        // OLEDB
                        try
                        {
                            var o = cn.OLEDBConnection;
                            if (o != null) src = o.Connection ?? "";
                        }
                        catch { }

                        // ODBC
                        if (string.IsNullOrWhiteSpace(src))
                        {
                            try
                            {
                                var odbc = cn.ODBCConnection;
                                if (odbc != null) src = odbc.Connection ?? "";
                            }
                            catch { }
                        }

                        // TEXT
                        if (string.IsNullOrWhiteSpace(src))
                        {
                            try
                            {
                                var txt = cn.TextConnection;
                                if (txt != null) src = txt.Connection ?? "";
                            }
                            catch { }
                        }

                        if (string.IsNullOrWhiteSpace(src)) src = cn.Name;

                        list.Add(new LinkItem
                        {
                            LinkSource = src,
                            SheetOrObject = "Connection",
                            CellOrDetail = cn.Name,
                            Type = "Connections",
                            TagLink = src
                        });
                    }
                }
                catch { }

                progressPercent?.Invoke(100);
                return list;
            }


            progressPercent?.Invoke(100);
            return list;
        }
        private int RemoveConnectionsForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            try
            {
                // لازم نمشي بالعكس عشان delete
                for (int i = wb.Connections.Count; i >= 1; i--)
                {
                    WorkbookConnection cn = wb.Connections.Item(i);
                    string s = "";

                    try { s = cn.OLEDBConnection?.Connection ?? ""; } catch { }
                    if (string.IsNullOrWhiteSpace(s)) { try { s = cn.ODBCConnection?.Connection ?? ""; } catch { } }
                    if (string.IsNullOrWhiteSpace(s)) { try { s = cn.TextConnection?.Connection ?? ""; } catch { } }
                    if (string.IsNullOrWhiteSpace(s)) { try { s = cn.Name ?? ""; } catch { } }

                    if (!string.IsNullOrWhiteSpace(s) && s.IndexOf(link, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try { cn.Delete(); count++; } catch { }
                    }
                }
            }
            catch { }

            return count;
        }
        private int RemoveQueriesForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            foreach (Worksheet ws in wb.Worksheets)
            {
                // QueryTables
                try
                {
                    foreach (QueryTable qt in ws.QueryTables)
                    {
                        try
                        {
                            string conn = qt.Connection?.ToString() ?? "";
                            if (conn.IndexOf(link, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                qt.Delete();
                                count++;
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                // ListObjects with QueryTable
                try
                {
                    foreach (ListObject lo in ws.ListObjects)
                    {
                        try
                        {
                            var qt = lo.QueryTable;
                            if (qt == null) continue;

                            string conn = qt.Connection?.ToString() ?? "";
                            if (conn.IndexOf(link, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                qt.Delete();
                                count++;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return count;
        }
        private int RefreshOrDetachPivotCachesForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            try
            {
                foreach (PivotCache pc in wb.PivotCaches())
                {
                    try
                    {
                        string src = pc.SourceData?.ToString() ?? "";
                        if (src.IndexOf(link, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // غالبًا الأفضل: Refresh / Make sure it's not external
                            // لكن مفيش Delete مباشر للـ cache. نعمل Refresh كحل، أو نسيبه.
                            // هنا هنعدّه كـ found.
                            count++;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return count;
        }

        public sealed class BreakResult
        {
            public bool Success { get; set; }
            public int ChangedCount { get; set; }
            public string? Error { get; set; }
        }
        public BreakResult BreakSingleLinkSources(Workbook wb, LinkItem item)
        {
            string link = item.TagLink ?? "";
            if (string.IsNullOrWhiteSpace(link))
                return new BreakResult { Success = false, Error = "Empty link" };

            Exception? last = null;

            try
            {
                wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks);
                return new BreakResult { Success = true, ChangedCount = 1 };
            }
            catch (Exception ex) { last = ex; }

            try
            {
                wb.BreakLink(link, XlLinkType.xlLinkTypeOLELinks);
                return new BreakResult { Success = true, ChangedCount = 1 };
            }
            catch (Exception ex) { last = ex; }

            return new BreakResult { Success = false, Error = last?.Message ?? "Unknown error" };
        }
        public Task<int> BreakLinksSmartAsync(
    Workbook wb,
    IList<LinkItem> selectedLinks,
    System.Action<LinkItem, string, bool, string?>? onItemUpdate,
    System.Action<int>? onProgress)
        {
            return StaTaskRunner.Run(() =>
            {
                int total = selectedLinks.Count;
                int done = 0;
                int totalChanged = 0;

                // Excel perf
                var oldCalc = _app.Calculation;
                var oldScreen = _app.ScreenUpdating;
                var oldEvents = _app.EnableEvents;

                _app.ScreenUpdating = false;
                _app.EnableEvents = false;
                _app.Calculation = XlCalculation.xlCalculationManual;

                try
                {
                    foreach (var item in selectedLinks)
                    {
                        done++;
                        string link = item.TagLink ?? item.LinkSource ?? "";

                        onItemUpdate?.Invoke(item, "Breaking...", true, null);
                        onProgress?.Invoke((int)((done - 1) * 100.0 / Math.Max(1, total)));

                        int changedForThis = 0;
                        string? lastErr = null;

                        try
                        {
                            // 1) Try BreakLink (ExcelLinks / OLELinks)
                            if (TryBreakLink(wb, link, out string? err1))
                            {
                                changedForThis += 1;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(err1)) lastErr = err1;

                                // 2) Formulas -> Values
                                changedForThis += ConvertFormulasForLink(wb, link);

                                // 3) Names
                                changedForThis += FixNamesForLink(wb, link);

                                // 4) Charts
                                changedForThis += FixChartsForLink(wb, link);

                                // 5) Shapes text
                                changedForThis += FixShapesForLink(wb, link);
                                changedForThis += RemoveConnectionsForLink(wb, link);
                                changedForThis += RemoveQueriesForLink(wb, link);
                                changedForThis += RefreshOrDetachPivotCachesForLink(wb, link);

                            }

                            totalChanged += changedForThis;

                            if (changedForThis > 0)
                                onItemUpdate?.Invoke(item, "Broken", false, null);
                            else
                                onItemUpdate?.Invoke(item, "Not found", false, lastErr);
                        }
                        catch (Exception ex)
                        {
                            onItemUpdate?.Invoke(item, "Failed", false, ex.Message);
                        }

                        onProgress?.Invoke((int)(done * 100.0 / Math.Max(1, total)));
                    }
                }
                finally
                {
                    _app.Calculation = oldCalc;
                    _app.EnableEvents = oldEvents;
                    _app.ScreenUpdating = oldScreen;

                    onProgress?.Invoke(100);
                }

                return totalChanged;
            });
        }
        public int BreakAllWorkbookLinks(Workbook wb, SysActionInt? progressPercent = null)
        {
            if (wb == null) return 0;

            int broken = 0;
            try
            {
                var arr = wb.LinkSources(XlLink.xlExcelLinks) as Array;
                if (arr == null || arr.Length == 0)
                {
                    progressPercent?.Invoke(100);
                    return 0;
                }

                int total = arr.Length;
                int done = 0;

                _app.ScreenUpdating = false;

                foreach (var o in arr)
                {
                    done++;
                    int p = (int)(done * 100.0 / Math.Max(1, total));
                    progressPercent?.Invoke(p);

                    string link = o?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(link)) continue;

                    try
                    {
                        wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks);
                        broken++;
                    }
                    catch
                    {
                        // بعض الروابط بتفشل لو already broken أو stale جدًا
                    }
                }

                progressPercent?.Invoke(100);
                return broken;
            }
            finally
            {
                _app.ScreenUpdating = true;
            }
        }

        private static bool TryBreakLink(Workbook wb, string link, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(link)) { error = "Empty link"; return false; }

            try
            {
                wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks);
                return true;
            }
            catch (Exception ex1)
            {
                try
                {
                    wb.BreakLink(link, XlLinkType.xlLinkTypeOLELinks);
                    return true;
                }
                catch (Exception ex2)
                {
                    error = $"BreakLink failed (Excel/OLE): {ex2.Message}";
                    // أحيانًا Excel/OLE الاتنين بيفشلوا، فنكمل للـ formulas/names...
                    return false;
                }
            }
        }
        private int ConvertFormulasForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            foreach (Worksheet ws in wb.Worksheets)
            {
                Range? formulas = null;
                try { formulas = ws.UsedRange.SpecialCells(XlCellType.xlCellTypeFormulas); }
                catch { formulas = null; }

                if (formulas == null) continue;

                foreach (Range c in formulas.Cells)
                {
                    try
                    {
                        string f = c.Formula?.ToString() ?? "";
                        if (!f.Contains(link)) continue;

                        c.Value2 = c.Value2; // تثبيت
                        count++;
                    }
                    catch { }
                }
            }

            return count;
        }
        private int FixNamesForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            try
            {
                int n = wb.Names.Count;
                for (int i = 1; i <= n; i++)
                {
                    try
                    {
                        var nm = wb.Names.Item(i);
                        string refersTo = nm.RefersTo ?? "";
                        if (!refersTo.Contains(link)) continue;

                        nm.RefersTo = nm.RefersTo; // تثبيت
                        count++;
                    }
                    catch { }
                }
            }
            catch { }

            return count;
        }
        private int FixChartsForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            foreach (Worksheet ws in wb.Worksheets)
            {
                try
                {
                    foreach (ChartObject co in ws.ChartObjects())
                    {
                        SeriesCollection sc;
                        try { sc = co.Chart.SeriesCollection(); }
                        catch { continue; }

                        int scCount = 0;
                        try { scCount = sc.Count; } catch { scCount = 0; }

                        for (int i = 1; i <= scCount; i++)
                        {
                            try
                            {
                                Series ser = sc.Item(i);
                                string sf = ser.Formula?.ToString() ?? "";
                                if (!sf.Contains(link)) continue;

                                ser.Values = ser.Values; // تثبيت
                                count++;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            return count;
        }
        private int FixShapesForLink(Workbook wb, string link)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(link)) return 0;

            foreach (Worksheet ws in wb.Worksheets)
            {
                try
                {
                    foreach (Shape shp in ws.Shapes)
                    {
                        try
                        {
                            if (!TryGetShapeText(shp, out string t)) continue;
                            if (string.IsNullOrWhiteSpace(t)) continue;
                            if (!t.Contains(link)) continue;

                            bool changed = false;

                            try
                            {
                                var tf2 = shp.TextFrame2;
                                if (tf2 != null && tf2.HasText == Microsoft.Office.Core.MsoTriState.msoTrue)
                                {
                                    tf2.TextRange.Text = tf2.TextRange.Text;
                                    changed = true;
                                }
                            }
                            catch { }

                            if (!changed)
                            {
                                try
                                {
                                    var tf = shp.TextFrame;
                                    if (tf != null)
                                    {
                                        var cur = tf.Characters().Text?.ToString() ?? "";
                                        tf.Characters().Text = cur;
                                        changed = true;
                                    }
                                }
                                catch { }
                            }

                            if (changed) count++;
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return count;
        }


        public int BreakSingleLink(Workbook wb, LinkItem item)
        {
            int count = 0;

            string link = item.TagLink ?? "";
            if (string.IsNullOrWhiteSpace(link)) return 0;

            // 1) BreakLink (LinkSources)
            try { wb.BreakLink(link, XlLinkType.xlLinkTypeExcelLinks); } catch { }

            // 2) Formulas -> values
            foreach (Worksheet ws in wb.Worksheets)
            {
                Range? formulas = null;
                try { formulas = ws.UsedRange.SpecialCells(XlCellType.xlCellTypeFormulas); }
                catch { formulas = null; }

                if (formulas == null) continue;

                foreach (Range c in formulas.Cells)
                {
                    try
                    {
                        string f = c.Formula?.ToString() ?? "";
                        if (!f.Contains(link)) continue;

                        c.Value2 = c.Value2;
                        count++;
                    }
                    catch { }
                }
            }

            // 3) Names
            try
            {
                int n = wb.Names.Count;
                var arr = new string[n + 1];
                for (int i = 1; i <= n; i++)
                {
                    try { arr[i] = wb.Names.Item(i).RefersTo ?? ""; }
                    catch { arr[i] = ""; }
                }

                for (int i = 1; i <= n; i++)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(arr[i])) continue;
                        if (!arr[i].Contains(link)) continue;

                        wb.Names.Item(i).RefersTo = wb.Names.Item(i).RefersTo;
                        count++;
                    }
                    catch { }
                }
            }
            catch { }

            // 4) Charts
            foreach (Worksheet ws in wb.Worksheets)
            {
                foreach (ChartObject co in ws.ChartObjects())
                {
                    SeriesCollection sc;
                    try { sc = co.Chart.SeriesCollection(); }
                    catch { continue; }

                    int scCount = 0;
                    try { scCount = sc.Count; } catch { scCount = 0; }

                    for (int i = 1; i <= scCount; i++)
                    {
                        try
                        {
                            Series ser = sc.Item(i);
                            string sf = ser.Formula?.ToString() ?? "";
                            if (!sf.Contains(link)) continue;

                            ser.Values = ser.Values;
                            count++;
                        }
                        catch { }
                    }
                }
            }

            // 5) Shapes
            foreach (Worksheet ws in wb.Worksheets)
            {
                foreach (Shape shp in ws.Shapes)
                {
                    try
                    {
                        if (!TryGetShapeText(shp, out string t)) continue;
                        if (string.IsNullOrEmpty(t)) continue;
                        if (!t.Contains(link)) continue;

                        bool changed = false;

                        try
                        {
                            var tf2 = shp.TextFrame2;
                            if (tf2 != null && tf2.HasText == Microsoft.Office.Core.MsoTriState.msoTrue)
                            {
                                tf2.TextRange.Text = tf2.TextRange.Text;
                                changed = true;
                            }
                        }
                        catch { }

                        if (!changed)
                        {
                            try
                            {
                                var tf = shp.TextFrame;
                                if (tf != null)
                                {
                                    var cur = tf.Characters().Text?.ToString() ?? "";
                                    tf.Characters().Text = cur;
                                    changed = true;
                                }
                            }
                            catch { }
                        }

                        if (changed) count++;
                    }
                    catch { }
                }
            }

            return count;
        }

        public Task<int> BreakLinksWithStatusAsync(
    Workbook wb,
    IList<LinkItem> selectedLinks,
    System.Action<LinkItem, string, bool, string?>? onItemStatus,
    System.Action<int>? onProgress)
        {
            return StaTaskRunner.Run(() =>
            {
                int total = selectedLinks.Count;
                int done = 0;
                int changedTotal = 0;

                _app.ScreenUpdating = false;

                try
                {
                    foreach (var item in selectedLinks)
                    {
                        done++;

                        onItemStatus?.Invoke(item, "Breaking...", true, null);
                        onProgress?.Invoke((int)((done - 1) * 100.0 / total));

                        // جرّب LinkSources فقط (حسب النوع)
                        BreakResult r = BreakSingleLinkSources(wb, item);

                        if (r.Success)
                        {
                            changedTotal += r.ChangedCount;
                            onItemStatus?.Invoke(item, "Broken", false, null);
                        }
                        else
                        {
                            onItemStatus?.Invoke(item, "Failed", false, r.Error);
                        }

                        onProgress?.Invoke((int)(done * 100.0 / total));
                    }
                }
                finally
                {
                    _app.ScreenUpdating = true;
                }

                return changedTotal;
            });
        }




    }
    public static class StaTaskRunner
    {
        public static Task Run(System.Action action)
            => Run<object?>(() => { action(); return null; });

        public static Task<T> Run<T>(System.Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            var thread = new Thread(() =>
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return tcs.Task;
        }
    }

}
