using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Models
{
    public class ActivityGroupingRule
    {
        public string Classification { get; set; } = string.Empty;
        public string TradeCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public static IReadOnlyList<ActivityGroupingRule> DefaultRules => new[]
        {
            new ActivityGroupingRule { Classification = "Concrete", TradeCategory = "Structure", Description = "Concrete works" },
            new ActivityGroupingRule { Classification = "Rebar", TradeCategory = "Structure", Description = "Reinforcement works" },
            new ActivityGroupingRule { Classification = "Formwork", TradeCategory = "Structure", Description = "Formwork works" },
            new ActivityGroupingRule { Classification = "Excavation", TradeCategory = "Site Preparation", Description = "Earthworks" },
            new ActivityGroupingRule { Classification = "Backfill", TradeCategory = "Site Preparation", Description = "Backfilling works" },
            new ActivityGroupingRule { Classification = "Block", TradeCategory = "Structure", Description = "Blockwork" },
            new ActivityGroupingRule { Classification = "Plaster", TradeCategory = "Finishes", Description = "Plastering works" },
            new ActivityGroupingRule { Classification = "Tiles", TradeCategory = "Finishes", Description = "Tiling works" },
            new ActivityGroupingRule { Classification = "Painting", TradeCategory = "Finishes", Description = "Painting works" },
            new ActivityGroupingRule { Classification = "Electrical", TradeCategory = "MEP", Description = "Electrical works" },
            new ActivityGroupingRule { Classification = "Plumbing", TradeCategory = "MEP", Description = "Plumbing works" },
            new ActivityGroupingRule { Classification = "HVAC", TradeCategory = "MEP", Description = "HVAC works" },
            new ActivityGroupingRule { Classification = "Roof", TradeCategory = "Roof", Description = "Roofing works" },
            new ActivityGroupingRule { Classification = "Landscape", TradeCategory = "Landscaping", Description = "Landscaping works" },
        };

        public static List<IGrouping<string, BOQItem>> GroupItems(IReadOnlyList<BOQItem> items)
        {
            return items.GroupBy(i => i.Classification).ToList();
        }
    }
}
