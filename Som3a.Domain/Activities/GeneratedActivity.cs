using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a.Domain.Activities
{
    public class GeneratedActivity
    {
        public string ActivityId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> BoqReferences { get; set; } = new();
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string WbsPath { get; set; } = string.Empty;
        public string TradeCategory { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
        public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;
        public bool IsUserModified { get; set; }
        public string? OriginalName { get; set; }
        public int SortOrder { get; set; }

        public string BoqReferencesDisplay =>
            BoqReferences != null ? string.Join(", ", BoqReferences) : string.Empty;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) && Quantity >= 0 && BoqReferences.Count > 0;
    }
}
