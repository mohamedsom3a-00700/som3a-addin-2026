using System.Text;
using Som3a.Domain.BOQ;
using Som3a.Domain.Activities;
using Som3a.Domain.WBS;

namespace Som3a.AI.Orchestration
{
    public class ContextBuilder
    {
        public string BuildContext(object entity)
        {
            return entity switch
            {
                BOQDocument boq => BuildBOQContext(boq),
                IEnumerable<Activity> activities => BuildActivityContext(activities),
                WBSNode wbs => BuildWBSContext(wbs),
                _ => entity.ToString() ?? string.Empty
            };
        }

        private string BuildBOQContext(BOQDocument boq)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"BOQ: {boq.ProjectName}");
            sb.AppendLine($"Sections: {boq.Sections.Count}, Total Items: {boq.TotalItems}");

            foreach (var section in boq.Sections)
            {
                sb.AppendLine($"  [{section.SectionCode}] {section.SectionName}: {section.Items.Count} items");
                foreach (var item in section.Items.Take(10))
                {
                    sb.AppendLine($"    - {item.ItemCode}: {item.Description} ({item.Quantity} {item.Unit})");
                }
                if (section.Items.Count > 10)
                    sb.AppendLine($"    ... and {section.Items.Count - 10} more items");
            }

            return sb.ToString();
        }

        private string BuildActivityContext(IEnumerable<Activity> activities)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Activities:");
            foreach (var a in activities)
            {
                sb.AppendLine($"  [{a.ActivityId}] {a.Name}");
                if (a.Duration.HasValue)
                    sb.AppendLine($"    Duration: {a.Duration}, Qty: {a.Quantity} {a.Unit}");
                sb.AppendLine($"    WBS: {a.WBSNode?.FullPath ?? "N/A"}");
            }
            sb.AppendLine($"Total: {activities.Count()} activities");
            return sb.ToString();
        }

        private string BuildWBSContext(WBSNode wbs)
        {
            var sb = new StringBuilder();
            BuildWBSLevel(sb, wbs, 0);
            return sb.ToString();
        }

        private void BuildWBSLevel(StringBuilder sb, WBSNode node, int indent)
        {
            var prefix = new string(' ', indent * 2);
            sb.AppendLine($"{prefix}[{node.Code}] {node.Name} (Level {node.Level})");
            foreach (var child in node.Children)
                BuildWBSLevel(sb, child, indent + 1);
        }
    }
}
