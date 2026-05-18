using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Som3a.Shared.Core
{
    public class XerParser
    {
        public List<XerTable> Tables { get; set; } = new();

        public void Parse(string filePath)
        {
            Tables.Clear();

            var lines = File.ReadLines(filePath);
            XerTable currentTable = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("%T"))
                {
                    currentTable = new XerTable
                    {
                        Name = line.Split('\t')[1]
                    };

                    Tables.Add(currentTable);
                }
                else if (line.StartsWith("%F") && currentTable != null)
                {
                    currentTable.Headers = line.Split('\t').Skip(1).ToList();
                }
                else if (line.StartsWith("%R") && currentTable != null)
                {
                    var row = line.Split('\t').Skip(1).ToList();
                    currentTable.Rows.Add(row);
                }
            }
        }

        public List<Activity> GetActivities()
        {
            var taskTable = Tables.FirstOrDefault(t => t.Name.ToUpper().Contains("TASK"));
            if (taskTable == null) return new List<Activity>();

            int idIdx = taskTable.Headers.FindIndex(h => h.Contains("task_id"));
            int codeIdx = taskTable.Headers.FindIndex(h => h.Contains("task_code"));
            int nameIdx = taskTable.Headers.FindIndex(h => h.Contains("task_name"));
            int floatIdx = taskTable.Headers.FindIndex(h => h.Contains("float"));
            int startIdx = taskTable.Headers.FindIndex(h => h.Contains("start"));
            int finishIdx = taskTable.Headers.FindIndex(h => h.Contains("end"));
            int wbsIdx = taskTable.Headers.FindIndex(h => h.Contains("wbs"));

            return taskTable.Rows.Select(r => new Activity
            {
                Id = idIdx >= 0 ? r[idIdx] : "",
                Code = codeIdx >= 0 ? r[codeIdx] : "",
                Name = nameIdx >= 0 ? r[nameIdx] : "",
                TotalFloat = floatIdx >= 0 && double.TryParse(r[floatIdx], out var f) ? f : 0,
                Start = startIdx >= 0 && DateTime.TryParse(r[startIdx], out var s) ? s : null,
                Finish = finishIdx >= 0 && DateTime.TryParse(r[finishIdx], out var e) ? e : null,
                WBSId = wbsIdx >= 0 ? r[wbsIdx] : ""
            }).ToList();
        }

        public Dictionary<string, List<Relationship>> GetRelationships()
        {
            var relTable = Tables.FirstOrDefault(t => t.Name == "TASKPRED");
            var dict = new Dictionary<string, List<Relationship>>();

            if (relTable == null) return dict;

            int predIdx = relTable.Headers.IndexOf("pred_task_id");
            int succIdx = relTable.Headers.IndexOf("task_id");
            int typeIdx = relTable.Headers.IndexOf("pred_type");

            foreach (var r in relTable.Rows)
            {
                var rel = new Relationship
                {
                    PredecessorId = r[predIdx],
                    SuccessorId = r[succIdx],
                    Type = r[typeIdx]
                };

                if (!dict.ContainsKey(rel.PredecessorId))
                    dict[rel.PredecessorId] = new List<Relationship>();

                dict[rel.PredecessorId].Add(rel);
            }

            return dict;
        }

        // 🔥 دى كانت ناقصة
        public List<WbsItem> GetWBS()
        {
            var table = Tables.FirstOrDefault(t => t.Name.ToUpper().Contains("WBS"));
            if (table == null) return new List<WbsItem>();

            int idIdx = table.Headers.FindIndex(h => h.Contains("wbs_id"));
            int parentIdx = table.Headers.FindIndex(h => h.Contains("parent"));
            int nameIdx = table.Headers.IndexOf("wbs_name");           // ✅ الاسم الصح
            int codeIdx = table.Headers.IndexOf("wbs_short_name");

            return table.Rows.Select(r => new WbsItem
            {
                Id = idIdx >= 0 ? r[idIdx] : "",
                ParentId = parentIdx >= 0 ? r[parentIdx] : "",
                Name = nameIdx >= 0 ? r[nameIdx] : "",
                WbsName = nameIdx >= 0 ? r[nameIdx] : "",        // 🔥 مهم
                WbsCode = codeIdx >= 0 ? r[codeIdx] : ""         // 🔥 مهم
            }).ToList();
        }
    }
}