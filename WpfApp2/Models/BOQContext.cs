using System.Collections.Generic;

namespace Som3a_WPF_UI.Models
{
    public class BOQContext
    {
        public string WorkbookName { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
        public List<BOQItem> Items { get; set; } = new();
        public int ItemCount => Items.Count;
        public decimal TotalQuantity { get; set; }
        public bool IsTruncated { get; set; }
        public int TruncatedItemCount { get; set; }
    }

    public class BOQItem
    {
        public string ItemNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string? Identifier { get; set; }
    }
}
