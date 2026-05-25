namespace Som3a.Domain.BOQ
{
    public class BOQDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ProjectName { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
        public List<BOQSection> Sections { get; set; } = new();
        public int TotalItems => Sections.Sum(s => s.Items.Count);
        public string? Source { get; set; }

        public void Validate()
        {
            var computedTotal = Sections.Sum(s => s.Items.Count);
            if (computedTotal != TotalItems)
                throw new InvalidOperationException(
                    $"BOQDocument TotalItems ({TotalItems}) doesn't match computed sum ({computedTotal}).");
        }
    }
}
