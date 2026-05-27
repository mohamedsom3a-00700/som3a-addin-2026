using System;
using System.Collections.Generic;
using System.Drawing;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSColorService
{
    private readonly List<Color> _levelColors = new()
    {
        Color.FromArgb(52, 73, 94),    // Level 0 - dark slate
        Color.FromArgb(41, 128, 185),  // Level 1 - blue
        Color.FromArgb(39, 174, 96),   // Level 2 - green
        Color.FromArgb(243, 156, 18),  // Level 3 - amber
        Color.FromArgb(231, 76, 60),   // Level 4 - red
        Color.FromArgb(155, 89, 182),  // Level 5 - purple
        Color.FromArgb(52, 152, 219),  // Level 6 - light blue
        Color.FromArgb(26, 188, 156),  // Level 7 - teal
        Color.FromArgb(230, 126, 34),  // Level 8 - orange
        Color.FromArgb(149, 165, 166), // Level 9+ - gray
    };

    public Color GetNodeColor(int level)
    {
        if (level < 0) return _levelColors[0];
        if (level >= _levelColors.Count) return _levelColors[_levelColors.Count - 1];
        return _levelColors[level];
    }

    public string GetNodeHexColor(int level)
    {
        var c = GetNodeColor(level);
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public Color GetConnectorColor(int fromLevel, int toLevel)
    {
        var avg = (fromLevel + toLevel) / 2;
        return GetNodeColor(avg);
    }

    public void ApplyLevelColorToExcelRow(Microsoft.Office.Interop.Excel.Range rowRange, int level)
    {
        var color = GetNodeColor(level);
        rowRange.Interior.Color = color;
        // Use white text for dark backgrounds, black for light
        var brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        rowRange.Font.Color = brightness < 0.5 ? Color.White : Color.Black;
    }
}
