using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public static class XerMapper
{
    public static List<Activity> GetActivities(List<XerTable> tables)
    {
        var taskTable = tables.FirstOrDefault(t => t.Name == "TASK");
        if (taskTable == null) return new();

        var list = new List<Activity>();

        foreach (var row in taskTable.Rows)
        {
            var dict = taskTable.Headers
                .Zip(row, (h, v) => new { h, v })
                .ToDictionary(x => x.h, x => x.v);

            list.Add(new Activity
            {
                Id = Get(dict, "task_id"),
                Code = !string.IsNullOrEmpty(Get(dict, "task_code"))
                        ? Get(dict, "task_code")
                        : Get(dict, "task_id"),

                Name = Get(dict, "task_name"),
                WBSId = Get(dict, "wbs_id"),
                TotalFloat = GetFloat(dict),

                Start = TryDate(dict, "early_start_date"),
                Finish = TryDate(dict, "early_end_date")
            });
        }

        return list;
    }

    public static List<Relationship> GetRelationships(List<XerTable> tables)
    {
        var relTable = tables.FirstOrDefault(t => t.Name == "TASKPRED");
        if (relTable == null) return new();

        var list = new List<Relationship>();

        foreach (var row in relTable.Rows)
        {
            var dict = relTable.Headers
                .Zip(row, (h, v) => new { h, v })
                .ToDictionary(x => x.h, x => x.v);

            list.Add(new Relationship
            {
                SuccessorId = Get(dict, "task_id"),
                PredecessorId = Get(dict, "pred_task_id"),
                Type = Get(dict, "pred_type"),
                Lag = TryDouble(dict, "lag_hr_cnt") / 8.0
            });
        }

        return list;
    }

    // ================= HELPERS =================

    private static string Get(Dictionary<string, string> d, string k)
        => d.ContainsKey(k) ? d[k] : "";

    private static double TryDouble(Dictionary<string, string> d, string k)
    {
        if (!d.ContainsKey(k)) return 0;
        var v = d[k]?.Replace(",", "");
        return double.TryParse(v, out var r) ? r : 0;
    }

    private static double GetFloat(Dictionary<string, string> d)
    {
        double hr = TryDouble(d, "total_float_hr_cnt");
        if (hr != 0) return hr / 8.0;

        return TryDouble(d, "total_float");
    }

    private static DateTime? TryDate(Dictionary<string, string> d, string k)
    {
        return d.ContainsKey(k) && DateTime.TryParse(d[k], out var dt)
            ? dt
            : null;
    }
}