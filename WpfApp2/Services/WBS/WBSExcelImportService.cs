using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSExcelImportService
{
    private readonly IWBSCodeGenerator _codeGen;
    private readonly IWBSTreeValidator _validator;
    private readonly WBSAIService? _aiService;

    public WBSExcelImportService(IWBSCodeGenerator codeGen, IWBSTreeValidator validator, WBSAIService? aiService = null)
    {
        _codeGen = codeGen;
        _validator = validator;
        _aiService = aiService;
    }

    public WBSNode ImportFromActiveSheet(object excelApp, Worksheet? sheet = null)
    {
        var app = (Microsoft.Office.Interop.Excel.Application)excelApp;
        var activeSheet = sheet ?? (Worksheet)app.ActiveSheet;
        var used = activeSheet.UsedRange;

        if (used == null || used.Rows.Count < 2)
            throw new InvalidOperationException("Active sheet has no data.");

        var rows = new List<Dictionary<string, string>>();
        var headers = new List<string>();

        // Read header row
        for (int c = 1; c <= used.Columns.Count; c++)
        {
            var val = (used.Cells[1, c] as Range)?.Value?.ToString() ?? $"Column{c}";
            headers.Add(val.ToLower());
        }

        // Read data rows
        for (int r = 2; r <= used.Rows.Count; r++)
        {
            var row = new Dictionary<string, string>();
            bool hasData = false;
            for (int c = 1; c <= headers.Count; c++)
            {
                var val = (used.Cells[r, c] as Range)?.Value?.ToString() ?? string.Empty;
                row[headers[c - 1]] = val;
                if (!string.IsNullOrWhiteSpace(val)) hasData = true;
            }
            if (hasData) rows.Add(row);
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("No data rows found in the active sheet.");

        // Try AI-powered import first
        if (_aiService != null && _aiService.IsAIAvailable)
        {
            try
            {
                var context = BuildAIContext(headers, rows);
                var wbs = _aiService.GenerateWBSFromExcelAsync(context).GetAwaiter().GetResult();
                _codeGen.RenumberSubtree(wbs);
                return wbs;
            }
            catch
            {
                // Fall through to rule-based import
            }
        }

        // Rule-based fallback: group by section/activity columns
        return BuildRuleBasedWbs(headers, rows);
    }

    private string BuildAIContext(List<string> headers, List<Dictionary<string, string>> rows)
    {
        var lines = new List<string>();
        lines.Add("Excel Columns: " + string.Join(", ", headers));
        lines.Add($"Total Rows: {rows.Count}");
        lines.Add("");
        lines.Add("Data:");
        foreach (var row in rows.Take(100))
        {
            var parts = headers.Select(h => $"{h}={row.GetValueOrDefault(h, "")}");
            lines.Add("  " + string.Join(", ", parts));
        }
        if (rows.Count > 100)
            lines.Add($"  ... and {rows.Count - 100} more rows");
        return string.Join(Environment.NewLine, lines);
    }

    private WBSNode BuildRuleBasedWbs(List<string> headers, List<Dictionary<string, string>> rows)
    {
        var root = new WBSNode { Code = "1", Name = "WBS from Excel" };

        // Try to find section and activity columns
        var sectionCol = headers.FirstOrDefault(h => h.Contains("section") || h.Contains("wbs") || h.Contains("level"));
        var activityCol = headers.FirstOrDefault(h => h.Contains("activity") || h.Contains("name") || h.Contains("item"));
        var codeCol = headers.FirstOrDefault(h => h.Contains("code") || h.Contains("id"));
        var qtyCol = headers.FirstOrDefault(h => h.Contains("qty") || h.Contains("quantity") || h.Contains("count"));

        if (sectionCol == null && activityCol == null)
        {
            // No recognized columns — use first column as section, second as activity
            root.Name = "Imported Data";
            var sectionGroups = rows.GroupBy(r => r.Values.FirstOrDefault() ?? "Unknown");
            int si = 1;
            foreach (var group in sectionGroups)
            {
                var section = new WBSNode { Code = $"{si}", Name = group.Key };
                int ai = 1;
                foreach (var row in group)
                {
                    var actName = row.Values.Skip(1).FirstOrDefault() ?? $"Item {ai}";
                    var act = new WBSNode { Code = $"{si}.{ai}", Name = actName };
                    section.Children.Add(act); act.Parent = section;
                    ai++;
                }
                root.Children.Add(section); section.Parent = root;
                si++;
            }
        }
        else
        {
            // Use detected columns
            var sectionNames = sectionCol != null
                ? rows.Select(r => r.GetValueOrDefault(sectionCol, "Unknown")).Distinct().ToList()
                : new List<string> { root.Name };

            int si = 1;
            foreach (var sectionName in sectionNames)
            {
                var section = new WBSNode { Code = $"{si}", Name = sectionName };
                var sectionRows = sectionCol != null
                    ? rows.Where(r => r.GetValueOrDefault(sectionCol, "Unknown") == sectionName)
                    : rows;

                int ai = 1;
                foreach (var row in sectionRows)
                {
                    var actName = activityCol != null ? row.GetValueOrDefault(activityCol, $"Item {ai}") : $"Item {ai}";
                    var act = new WBSNode { Code = $"{si}.{ai}", Name = actName };
                    if (codeCol != null && row.TryGetValue(codeCol, out var code) && !string.IsNullOrWhiteSpace(code))
                        act.Code = code;
                    section.Children.Add(act); act.Parent = section;
                    ai++;
                }
                root.Children.Add(section); section.Parent = root;
                si++;
            }
        }

        _codeGen.RenumberSubtree(root);
        return root;
    }

}

internal static class DictionaryExtensions
{
    public static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string defaultValue)
        => dict.TryGetValue(key, out var val) ? val : defaultValue;
}
