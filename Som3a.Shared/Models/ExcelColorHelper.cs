using System;
using System.Drawing;
using Excel = Microsoft.Office.Interop.Excel;

public static class ExcelColorHelper
{
    public static string OleToHex(int oleColor)
    {
        Color c = ColorTranslator.FromOle(oleColor);
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public static string GetFillHex(Excel.Range cell)
    {
        if (cell == null) return "";
        int ole = Convert.ToInt32(cell.Interior.Color);
        return OleToHex(ole);
    }

    public static string GetFontHex(Excel.Range cell)
    {
        if (cell == null) return "";
        int ole = Convert.ToInt32(cell.Font.Color);
        return OleToHex(ole);
    }
}
public sealed class PaletteExportService
{
    /// <summary>
    /// يكتب Fill HEX في العمود اللي جنب الخلية (يمينها)،
    /// واختياري يكتب Font HEX في العمود اللي بعده.
    /// </summary>
    public void ExportSelectedRangeToAdjacentCells(Excel.Application app, bool writeFontHexAlso)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        Excel.Range sel = app.Selection as Excel.Range;
        if (sel == null) return;

        // لو المستخدم محدد أكتر من area (مثلاً Ctrl+select) بنلف عليهم
        foreach (Excel.Range area in sel.Areas)
        {
            int rows = area.Rows.Count;
            int cols = area.Columns.Count;

            // هنكتب لكل خلية في الرنج
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    Excel.Range cell = area.Cells[r, c] as Excel.Range;
                    if (cell == null) continue;

                    string fillHex = ExcelColorHelper.GetFillHex(cell);

                    // اكتب Fill في الخلية اللي على يمينها
                    Excel.Range outCell = cell.Offset[0, 1] as Excel.Range;
                    if (outCell != null) outCell.Value2 = fillHex;

                    // اختياري: اكتب Font في اللي بعدها
                    if (writeFontHexAlso)
                    {
                        string fontHex = ExcelColorHelper.GetFontHex(cell);
                        Excel.Range outCell2 = cell.Offset[0, 2] as Excel.Range;
                        if (outCell2 != null) outCell2.Value2 = fontHex;
                    }
                }
            }
        }
    }
}
