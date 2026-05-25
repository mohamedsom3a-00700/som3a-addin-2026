namespace Som3a.Domain.BOQ
{
    public class BOQSection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string SectionName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<BOQItem> Items { get; set; } = new();
        public BOQDocument? ParentDocument { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SectionCode))
                throw new InvalidOperationException("BOQSection SectionCode must not be empty.");
            if (ParentDocument != null)
            {
                var duplicate = ParentDocument.Sections
                    .Any(s => s != this && s.DisplayOrder == DisplayOrder);
                if (duplicate)
                    throw new InvalidOperationException(
                        $"BOQSection DisplayOrder {DisplayOrder} already exists in document.");
            }
        }
    }
}
