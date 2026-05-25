namespace Som3a.Domain.BOQ
{
    public class BOQItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string? Classification { get; set; }
        public string? BOQReference { get; set; }
        public BOQSection? ParentSection { get; set; }

        public void Validate()
        {
            if (Quantity <= 0)
                throw new InvalidOperationException("BOQItem Quantity must be greater than 0.");
            if (string.IsNullOrWhiteSpace(Unit))
                throw new InvalidOperationException("BOQItem Unit must not be empty.");
            if (string.IsNullOrWhiteSpace(ItemCode))
                throw new InvalidOperationException("BOQItem ItemCode must not be empty.");
        }
    }
}
