using System;
using System.Collections.Generic;
using System.Drawing;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a.Shared.Core
{
    public sealed class FixPieColorsService
    {
        private readonly Dictionary<string, int> _colorDict;

        public FixPieColorsService()
        {
            _colorDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Storm & SewageDrainage Network"] = ColorTranslator.ToOle(Color.FromArgb(91, 155, 213)),
                ["Irrigation Network"] = ColorTranslator.ToOle(Color.FromArgb(237, 125, 49)),
                ["Potable Water & External Firefighting Network"] = ColorTranslator.ToOle(Color.FromArgb(165, 165, 165)),
                ["Electrical Network"] = ColorTranslator.ToOle(Color.FromArgb(255, 192, 0)),
                ["Telecom Network"] = ColorTranslator.ToOle(Color.FromArgb(68, 114, 196)),
                ["Communication System Network"] = ColorTranslator.ToOle(Color.FromArgb(112, 173, 71)),
                ["Process Control System"] = ColorTranslator.ToOle(Color.FromArgb(37, 94, 145)),
                ["Road Work"] = ColorTranslator.ToOle(Color.FromArgb(192, 80, 77)),
            };
        }

        public FixPieColorsResult ApplyColors(
    Excel.Application xlApp,
    string sheetName,
    string categoryRangeAddress,
    string colorTableRangeAddress
)
        {
            if (xlApp == null) throw new ArgumentNullException(nameof(xlApp));
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentNullException(nameof(sheetName));
            if (string.IsNullOrWhiteSpace(categoryRangeAddress)) throw new ArgumentNullException(nameof(categoryRangeAddress));
            if (string.IsNullOrWhiteSpace(colorTableRangeAddress)) throw new ArgumentNullException(nameof(colorTableRangeAddress));

            var result = new FixPieColorsResult();

            var wb = xlApp.ActiveWorkbook ?? throw new InvalidOperationException("No active workbook.");
            var ws = (Excel.Worksheet)wb.Worksheets[sheetName];

            // 1) Read Category labels range (list of labels in order)
            Excel.Range categoryRange = ws.Range[categoryRangeAddress];
            int catRows = categoryRange.Rows.Count;

            // 2) Read Color table: 2 columns (Label | ColorCellFill)
            Excel.Range colorTable = ws.Range[colorTableRangeAddress];

            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            int rows = colorTable.Rows.Count;
            for (int r = 1; r <= rows; r++)
            {
                string label = NormalizeLabel(Convert.ToString(((Excel.Range)colorTable.Cells[r, 1]).Value2));
                if (string.IsNullOrEmpty(label)) continue;

                var colorCell = (Excel.Range)colorTable.Cells[r, 2];

                // لو الخلية مش متلونة أحيانًا ترجع -4142 (xlNone) أو 0 حسب الحالة
                int oleColor;
                try
                {
                    oleColor = Convert.ToInt32(colorCell.Interior.Color);
                }
                catch
                {
                    continue;
                }

                dict[label] = oleColor;
            }

            var notMatched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 3) Apply to embedded charts in selected sheet
            foreach (Excel.ChartObject chObj in ws.ChartObjects())
            {
                result.ChartsCount++;

                Excel.Chart chart = chObj.Chart;
                Excel.SeriesCollection seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();

                foreach (Excel.Series srs in seriesCollection)
                {
                    result.SeriesCount++;

                    int pointsCount = srs.Points().Count;
                    int loopCount = Math.Min(pointsCount, catRows);

                    for (int i = 1; i <= loopCount; i++) // Excel 1-based
                    {
                        result.PointsTotal++;

                        string label = NormalizeLabel(Convert.ToString(((Excel.Range)categoryRange.Cells[i, 1]).Value2));
                        if (string.IsNullOrEmpty(label)) continue;

                        if (dict.TryGetValue(label, out int oleColor))
                        {
                            result.Matched++;

                            Excel.Point pt = (Excel.Point)srs.Points(i);

                            // نفس VBA
                            try
                            {
                                dynamic dpt = pt;
                                dpt.Format.Fill.Solid();
                                dpt.Format.Fill.ForeColor.RGB = oleColor;
                            }
                            catch
                            {
                                // fallback
                                try { pt.Interior.Color = oleColor; } catch { }
                            }

                            result.Updated++;
                        }
                        else
                        {
                            notMatched.Add(label);
                        }
                    }
                }
            }

            result.NotMatchedLabels.AddRange(notMatched);

            try { wb.RefreshAll(); } catch { }
            try { xlApp.Calculate(); } catch { }

            return result;
        }



        private static string NormalizeLabel(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            string x = s.Replace('\u00A0', ' ')
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Trim();

            while (x.Contains("  "))
                x = x.Replace("  ", " ");

            return x;
        }

        private static string? GetCategoryLabel(Excel.Series series, int pointIndex1Based)
        {
            try
            {
                // مثال Formula:
                // =SERIES(,Sheet1!$B$1:$I$1,Sheet1!$B$2:$I$2,1)

                string formula = series.Formula;
                if (string.IsNullOrWhiteSpace(formula))
                    return null;

                int firstComma = formula.IndexOf(',');
                int secondComma = formula.IndexOf(',', firstComma + 1);
                int thirdComma = formula.IndexOf(',', secondComma + 1);

                if (secondComma < 0 || thirdComma < 0)
                    return null;

                // الجزء اللي فيه XValues Range
                string xRangePart = formula.Substring(secondComma + 1, thirdComma - secondComma - 1);

                Excel.Range xRange = series.Application.Range[xRangePart];

                object val = xRange.Cells[1, pointIndex1Based].Value2;
                return Convert.ToString(val);
            }
            catch
            {
                return null;
            }
        }




    }
}
