using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace Som3a.Shared.Core
{
    public class WbsBuilder
    {
        private void CalculateWbsLevels(List<WbsItem> wbs)
        {
            var dict = wbs.ToDictionary(x => x.Id);

            foreach (var item in wbs)
            {
                int level = 1;
                var current = item;

                while (!string.IsNullOrEmpty(current.ParentId) && dict.ContainsKey(current.ParentId))
                {
                    level++;
                    current = dict[current.ParentId];
                }

                item.WbsLevel = level;
            }
        }
        private void BuildWbsPath(List<WbsItem> wbs)
        {
            var dict = wbs.ToDictionary(x => x.Id);

            foreach (var item in wbs)
            {
                var parts = new List<string>();

                var current = item;

                while (current != null)
                {
                    parts.Add($"{current.WbsCode} - {current.WbsName}");

                    if (string.IsNullOrEmpty(current.ParentId) || !dict.ContainsKey(current.ParentId))
                        break;

                    current = dict[current.ParentId];
                }

                parts.Reverse();

                item.FullPath = string.Join(" / ", parts);
            }
        }
        public List<WbsItem> BuildTree(List<WbsItem> wbs, List<Activity> acts)
        {
            var cleanWbs = wbs
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
            CalculateWbsLevels(cleanWbs);
            BuildWbsPath(cleanWbs);
            var dict = cleanWbs.ToDictionary(x => x.Id);

            var styles = WbsLevelStyle.Default15();

            // تنظيف + تلوين
            foreach (var item in cleanWbs)
            {
                item.Items.Clear();

                int lvl = item.WbsLevel;

                if (lvl <= 0 || lvl > 15)
                    lvl = item.Id?.Split('.').Length ?? 1;

                var style = styles[lvl];

                item.Background = new SolidColorBrush(
                    Color.FromRgb(style.Fill.R, style.Fill.G, style.Fill.B));

                item.Foreground = new SolidColorBrush(
                    Color.FromRgb(style.Font.R, style.Font.G, style.Font.B));
            }

            // ربط WBS
            foreach (var item in cleanWbs)
            {
                if (!string.IsNullOrEmpty(item.ParentId) && dict.ContainsKey(item.ParentId))
                {
                    dict[item.ParentId].Items.Add(item);
                }
            }

            // ربط Activities
            foreach (var a in acts)
            {
                if (!string.IsNullOrEmpty(a.WBSId) && dict.ContainsKey(a.WBSId))
                {
                    dict[a.WBSId].Items.Add(a);
                }
            }

            // Roots
            return cleanWbs
                .Where(x =>
                    string.IsNullOrEmpty(x.ParentId) ||
                    x.ParentId == "0" ||
                    !cleanWbs.Any(p => p.Id == x.ParentId))
                .ToList();
        }
    }
}