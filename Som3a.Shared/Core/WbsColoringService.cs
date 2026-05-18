using Microsoft.Office.Interop.Excel;
using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Som3a.Shared.Core
{
    
    public sealed class WbsColoringService
    {
        private readonly IReadOnlyDictionary<int, WbsLevelStyle> _styles;

        public WbsColoringService()
            : this(WbsLevelStyle.Default15())
        {
        }

        public WbsColoringService(IReadOnlyDictionary<int, WbsLevelStyle> styles)
        {
            _styles = styles ?? WbsLevelStyle.Default15();
        }

        public void ApplyToActiveSheet(Application app, string headerName, int maxLevels)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var ws = app.ActiveSheet as Worksheet;
            if (ws == null) return;

            Apply(ws, headerName, maxLevels);
        }

        public void Apply(Worksheet ws, string headerName, int maxLevels)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            if (string.IsNullOrWhiteSpace(headerName)) headerName = "Activity ID";
            if (maxLevels <= 0) maxLevels = 15;

            Range used = ws.UsedRange;
            if (used == null) return;

            int lastRow = used.Rows.Count + used.Row - 1;
            int lastCol = used.Columns.Count + used.Column - 1;

            int headerRow, activityIdCol;
            if (!TryFindHeader(ws, headerName, lastRow, lastCol, out headerRow, out activityIdCol))
                return;

            int tmpRow, activityNameCol;
            if (!TryFindHeader(ws, "Activity Name", lastRow, lastCol, out tmpRow, out activityNameCol))
                return;

            int dataStartRow = headerRow + 1;
            if (dataStartRow > lastRow) return;

            // كل كام مسافة = مستوى؟ (لو عندك كل مستوى = مسافتين خليها 2)
            int spacesPerLevel = 1;

            // العمود K
            int levelNameCol = 11;

            for (int r = dataStartRow; r <= lastRow; r++)
            {
                Range rowRange = ws.Range[ws.Cells[r, 1], ws.Cells[r, lastCol]];

                // شرط Activity Name: لو فيه كلام => الصف مايتلونش وامسح أي لون قديم + امسح اسم المستوى
                Range nameCell = ws.Cells[r, activityNameCol];
                string nameText = Convert.ToString(nameCell.Value2) ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(nameText))
                {
                    rowRange.Interior.Pattern = XlPattern.xlPatternNone;
                    rowRange.Interior.ColorIndex = XlColorIndex.xlColorIndexNone;
                    rowRange.Font.ColorIndex = XlColorIndex.xlColorIndexAutomatic;
                    rowRange.Font.Bold = false;
                    rowRange.Font.Italic = false;
                    ApplyLightBorders(rowRange);
                    // اختياري: شيل البوردرز لو الصف مش WBS
                    // rowRange.Borders.LineStyle = XlLineStyle.xlLineStyleNone;


                    // ws.Cells[r, 11].Value2 = ""; // K
                    continue;
                }


                // Activity ID: احسب المستوى من مسافات الشمال فقط (بدون Trim)
                Range idCell = ws.Cells[r, activityIdCol];

                string idRaw = Convert.ToString(idCell.Text) ?? string.Empty;
                if (idRaw.Length == 0)
                    idRaw = Convert.ToString(idCell.Value2) ?? string.Empty;

                int level = GetLevelFromLeftSpaces(idRaw, spacesPerLevel, maxLevels);

                WbsLevelStyle style;
                if (!_styles.TryGetValue(level, out style))
                {
                    ws.Cells[r, 11].Value2 = "";
                    continue;
                }

                // Solid fill (بدون Gradient)
                rowRange.Interior.Pattern = XlPattern.xlPatternSolid;
                rowRange.Interior.Color = ColorTranslator.ToOle(style.Fill);
                rowRange.Font.Color = ColorTranslator.ToOle(style.Font);
                // كل الخطوط Regular
                rowRange.Font.Bold = false;
                rowRange.Font.Italic = false;

                // Border خفيف لكل الخلايا في الصف
                ApplyLightBorders(rowRange);


                // اكتب اسم المستوى في K
                // ws.Cells[r, 11].Value2 = "WBS Level " + level;

            }
        }
        private static void ApplyLightBorders(Range rowRange)
        {
            // Border خفيف (رمادي فاتح)
            rowRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            rowRange.Borders.Weight = XlBorderWeight.xlThin;
            rowRange.Borders.Color = ColorTranslator.ToOle(Color.FromArgb(0, 0, 0));
        }

        // ===== Left spaces only (leading spaces) =====
        private static int CountLeftSpacesOnly(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;

            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];

                if (ch == ' ') { count++; continue; }        // normal space
                if (ch == '\u00A0') { count++; continue; }   // NBSP
                if (ch == '\t') { count += 1; continue; }    // tab = 1 space

                break;
            }

            return count;
        }

        private static int GetLevelFromLeftSpaces(string activityIdRaw, int spacesPerLevel, int maxLevels)
        {
            if (spacesPerLevel <= 0) spacesPerLevel = 1;

            int leftSpaces = CountLeftSpacesOnly(activityIdRaw);

            int level = (leftSpaces / spacesPerLevel) + 0;

            if (level < 1) level = 1;
            if (level > maxLevels) level = maxLevels;

            return level;
        }

        // Optional: لو عندك شيت متلوّن بالفعل وعايز تكتب أسماء المستويات من اللون
        public void WriteWbsLevelNameByRowColor(Worksheet ws, int targetColumn)
        {
            if (ws == null) return;

            Range used = ws.UsedRange;
            if (used == null) return;

            int lastRow = used.Rows.Count + used.Row - 1;
            int firstCol = used.Column;

            for (int r = used.Row; r <= lastRow; r++)
            {
                // اقرأ لون خلية داخل الـ UsedRange بدل العمود 1
                Range cell = ws.Cells[r, firstCol];

                int ole = Convert.ToInt32(cell.Interior.Color);
                Color rowColor = ColorTranslator.FromOle(ole);

                int level = GetLevelFromColor(rowColor);

                ws.Cells[r, targetColumn].Value2 = (level > 0) ? ("WBS Level " + level) : "";
            }
        }

        private int GetLevelFromColor(Color c)
        {
            foreach (var kv in _styles)
            {
                if (ColorsClose(kv.Value.Fill, c))
                    return kv.Key;
            }
            return 0;
        }

        private bool ColorsClose(Color a, Color b)
        {
            int tol = 3;
            return Math.Abs(a.R - b.R) <= tol &&
                   Math.Abs(a.G - b.G) <= tol &&
                   Math.Abs(a.B - b.B) <= tol;
        }

        private static bool TryFindHeader(Worksheet ws, string headerName, int lastRow, int lastCol,
                                         out int headerRow, out int headerCol)
        {
            headerRow = 0;
            headerCol = 0;

            int rMax = Math.Min(lastRow, 50);
            int cMax = Math.Min(lastCol, 100);

            for (int r = 1; r <= rMax; r++)
            {
                for (int c = 1; c <= cMax; c++)
                {
                    var v = ws.Cells[r, c].Value2;
                    var t = Convert.ToString(v);
                    if (string.IsNullOrEmpty(t)) continue;

                    t = t.Trim();
                    if (string.Equals(t, headerName, StringComparison.OrdinalIgnoreCase))
                    {
                        headerRow = r;
                        headerCol = c;
                        return true;
                    }
                }
            }

            return false;
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bb = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, g, bb);
        }
        private static void ApplyVerticalGradientShapeToRow(Worksheet ws, int row, int firstCol, int lastCol, Color baseColor)
        {
            // احسب أبعاد الصف (من أول عمود لآخر عمود)
            Range leftCell = ws.Cells[row, firstCol];
            Range rightCell = ws.Cells[row, lastCol];

            double left = (double)leftCell.Left;
            double top = (double)leftCell.Top;
            double width = (double)(rightCell.Left + rightCell.Width - leftCell.Left);
            double height = (double)leftCell.Height;

            // اسم ثابت للـ shape عشان لو اتكرر التلوين نمسح القديم
            string shpName = "Som3a_WBS_GR_" + row.ToString();

            // احذف الشكل القديم لو موجود
            try
            {
                ws.Shapes.Item(shpName).Delete();
            }
            catch { }

            // ارسم مستطيل فوق الصف
            var shp = ws.Shapes.AddShape(
                Microsoft.Office.Core.MsoAutoShapeType.msoShapeRectangle,
                (float)left, (float)top, (float)width, (float)height);

            shp.Name = shpName;

            // بدون حدود
            shp.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

            // خلي الشكل شفاف من ناحية النص (النص فوقه)
            shp.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;

            // Gradient رأسي: من أفتح (فوق) لأغمق (تحت) من نفس اللون
            Color topC = Lighten(baseColor, 0.35);
            Color botC = Darken(baseColor, 0.20);

            shp.Fill.OneColorGradient(
                Microsoft.Office.Core.MsoGradientStyle.msoGradientVertical,
                1, 0.5f);

            // هنا بنغير لون الـ gradient
            // GradientStops مش متاحة في بعض الإصدارات، فنعملها بالطريقة الآمنة:
            shp.Fill.ForeColor.RGB = ColorToOleRgb(topC);
            shp.Fill.BackColor.RGB = ColorToOleRgb(botC);

            // خلي الشكل يتحرك/يكبر مع الخلايا
            shp.Placement = XlPlacement.xlMoveAndSize;

            // ابعت الشكل للخلف عشان النص يبان فوقه
            shp.ZOrder(Microsoft.Office.Core.MsoZOrderCmd.msoSendToBack);
        }
        
        private static int ColorToOleRgb(Color c)
        {
            // Office RGB is 0x00BBGGRR
            return (c.B << 16) | (c.G << 8) | c.R;
        }

        private static Color Lighten(Color c, double amount)
        {
            int r = (int)(c.R + (255 - c.R) * amount);
            int g = (int)(c.G + (255 - c.G) * amount);
            int b = (int)(c.B + (255 - c.B) * amount);
            return Color.FromArgb(r, g, b);
        }

        private static Color Darken(Color c, double amount)
        {
            int r = (int)(c.R * (1 - amount));
            int g = (int)(c.G * (1 - amount));
            int b = (int)(c.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
        public void GroupRowsByWbsLevel(Worksheet ws, string headerName, int maxLevels, int spacesPerLevel)
        {
            if (ws == null) return;
            if (string.IsNullOrWhiteSpace(headerName)) headerName = "Activity ID";
            if (maxLevels <= 0) maxLevels = 15;
            if (spacesPerLevel <= 0) spacesPerLevel = 1;

            Range used = ws.UsedRange;
            if (used == null) return;

            int lastRow = used.Rows.Count + used.Row - 1;
            int lastCol = used.Columns.Count + used.Column - 1;

            int headerRow, activityIdCol;
            if (!TryFindHeader(ws, headerName, lastRow, lastCol, out headerRow, out activityIdCol))
                return;

            int tmpRow, activityNameCol;
            if (!TryFindHeader(ws, "Activity Name", lastRow, lastCol, out tmpRow, out activityNameCol))
                return;

            int dataStartRow = headerRow + 1;
            if (dataStartRow > lastRow) return;

            // 1) امسح أي Outline قديم
            try { ws.Rows.ClearOutline(); } catch { }

            // 2) خلي Summary فوق التفاصيل (زر +/-)
            ws.Outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            ws.Outline.AutomaticStyles = false;

            // 3) جهّز Arrays للمستويات ونوع الصف
            int n = lastRow + 1;
            int[] lvl = new int[n];
            bool[] isSummary = new bool[n];

            for (int r = dataStartRow; r <= lastRow; r++)
            {
                Range nameCell = ws.Cells[r, activityNameCol];
                string nameText = Convert.ToString(nameCell.Value2) ?? string.Empty;

                // Summary rows هي اللي Activity Name فاضي
                bool summary = string.IsNullOrWhiteSpace(nameText);
                isSummary[r] = summary;

                Range idCell = ws.Cells[r, activityIdCol];

                string idRaw = Convert.ToString(idCell.Text) ?? string.Empty;
                if (idRaw.Length == 0) idRaw = Convert.ToString(idCell.Value2) ?? string.Empty;

                // مستوى من مسافات الشمال فقط
                int level = GetLevelFromLeftSpaces(idRaw, spacesPerLevel, maxLevels);
                lvl[r] = level;
            }

            // 4) اعمل Group من تحت لفوق عشان الـ nesting يبقى صح
            for (int r = lastRow; r >= dataStartRow; r--)
            {
                if (!isSummary[r]) continue;

                int curLevel = lvl[r];

                // نهاية التفاصيل: لحد قبل أول Summary بعده مستوى <= curLevel
                int end = r;
                for (int k = r + 1; k <= lastRow; k++)
                {
                    if (isSummary[k] && lvl[k] <= curLevel)
                    {
                        end = k - 1;
                        break;
                    }
                    end = k;
                }

                int startDetails = r + 1;

                // لو مفيش تفاصيل تحته، سيبه
                if (startDetails > end) continue;

                // Group التفاصيل تحت الصف الحالي (Summary Above Detail)
                Range detailRows = ws.Range[ws.Rows[startDetails], ws.Rows[end]];
                detailRows.Rows.Group();
            }

            // اختياري: افتح كل الـ groups
            // ws.Outline.ShowLevels(RowLevels: 8);
        }
        public void GroupRowsFull(Worksheet ws, string activityIdHeader, int maxLevels, int spacesPerLevel, bool preferColorLevels)
        {
            if (ws == null) return;
            if (string.IsNullOrWhiteSpace(activityIdHeader)) activityIdHeader = "Activity ID";
            if (maxLevels <= 0) maxLevels = 15;
            if (spacesPerLevel <= 0) spacesPerLevel = 1;

            Range used = ws.UsedRange;
            if (used == null) return;

            int lastRow = used.Rows.Count + used.Row - 1;
            int lastCol = used.Columns.Count + used.Column - 1;

            int headerRow, activityIdCol;
            if (!TryFindHeader(ws, activityIdHeader, lastRow, lastCol, out headerRow, out activityIdCol))
                return;

            int tmpRow, activityNameCol;
            if (!TryFindHeader(ws, "Activity Name", lastRow, lastCol, out tmpRow, out activityNameCol))
                return;

            int dataStartRow = headerRow + 1;
            if (dataStartRow > lastRow) return;

            // 1) امسح أي Outline قديم
            try { ws.Rows.ClearOutline(); } catch { }

            // 2) Summary Above Detail (+/-)
            ws.Outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            ws.Outline.AutomaticStyles = false;

            // 3) جهز المستوى لكل صف + هل هو Summary
            int n = lastRow + 1;
            int[] lvl = new int[n];
            bool[] isSummary = new bool[n];

            // helper: اقرأ لون صف من أول عمود في usedrange (أضمن)
            int firstCol = used.Column;

            for (int r = dataStartRow; r <= lastRow; r++)
            {
                Range nameCell = ws.Cells[r, activityNameCol];
                string nameText = Convert.ToString(nameCell.Value2) ?? string.Empty;

                // Summary rows هي اللي Activity Name فاضي
                bool summary = string.IsNullOrWhiteSpace(nameText);
                isSummary[r] = summary;

                // level by spaces
                int levelBySpaces = 1;
                {
                    Range idCell = ws.Cells[r, activityIdCol];
                    string idRaw = Convert.ToString(idCell.Text) ?? string.Empty;
                    if (idRaw.Length == 0) idRaw = Convert.ToString(idCell.Value2) ?? string.Empty;
                    levelBySpaces = GetLevelFromLeftSpaces(idRaw, spacesPerLevel, maxLevels);
                }

                // level by color (0 if not matched)
                int levelByColor = 0;
                {
                    Range c = ws.Cells[r, firstCol];
                    int ole = Convert.ToInt32(c.Interior.Color);
                    Color rowColor = ColorTranslator.FromOle(ole);
                    levelByColor = GetLevelFromColor(rowColor);
                }

                int finalLevel;
                if (preferColorLevels)
                {
                    finalLevel = (levelByColor > 0) ? levelByColor : levelBySpaces;
                }
                else
                {
                    // prefer spaces first, fallback to color
                    finalLevel = (levelBySpaces > 0) ? levelBySpaces : ((levelByColor > 0) ? levelByColor : 1);
                }

                if (finalLevel < 1) finalLevel = 1;
                if (finalLevel > maxLevels) finalLevel = maxLevels;

                lvl[r] = finalLevel;
            }

            // 4) Full grouping: من تحت لفوق، لكل Summary اعمل group لتفاصيله
            for (int r = lastRow; r >= dataStartRow; r--)
            {
                if (!isSummary[r]) continue;

                int curLevel = lvl[r];

                int end = r;
                for (int k = r + 1; k <= lastRow; k++)
                {
                    if (isSummary[k] && lvl[k] <= curLevel)
                    {
                        end = k - 1;
                        break;
                    }
                    end = k;
                }

                int startDetails = r + 1;
                if (startDetails > end) continue;

                Range detailRows = ws.Range[ws.Rows[startDetails], ws.Rows[end]];
                detailRows.Rows.Group();
            }

            // اختياري: افتح كل المستويات (مش ضروري)
            // ws.Outline.ShowLevels(RowLevels: maxLevels);
        }



    }
}
