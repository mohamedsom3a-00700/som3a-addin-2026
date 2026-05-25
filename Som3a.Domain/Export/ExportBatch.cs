namespace Som3a.Domain.Export
{
    public enum ExportTarget
    {
        Excel,
        Csv,
        Json,
        Xml,
        PrimaveraCompatible
    }

    public class ExportBatch
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public ExportTarget ExportTarget { get; set; }
        public List<object> Data { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
