namespace Som3a.Domain.Resources
{
    public enum ResourceType
    {
        Labor,
        Equipment,
        Material,
        Subcontractor
    }

    public class Resource
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public ResourceType ResourceType { get; set; }
        public decimal CostPerHour { get; set; }
        public decimal Budget { get; set; }
        public List<ResourceAssignment> Assignments { get; set; } = new();
    }
}
