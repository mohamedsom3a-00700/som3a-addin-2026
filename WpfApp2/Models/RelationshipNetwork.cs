using System.Collections.Generic;

namespace Som3a_WPF_UI.Models
{
    public class RelationshipNetwork
    {
        public List<Relationship> Relationships { get; set; } = new();
        public List<ActivityItem> Activities { get; set; } = new();

        public bool HasCycles() => false;
    }

    public class ActivityItem
    {
        public string ActivityId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TradeCategory { get; set; } = string.Empty;
    }
}
