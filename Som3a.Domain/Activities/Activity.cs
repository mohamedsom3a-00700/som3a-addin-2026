using Som3a.Domain.Calendars;
using Som3a.Domain.Constraints;
using Som3a.Domain.Relationships;
using Som3a.Domain.Resources;
using Som3a.Domain.WBS;

namespace Som3a.Domain.Activities
{
    public class Activity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ActivityId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public TimeSpan? Duration { get; set; }
        public decimal ProductivityRate { get; set; }
        public WBS.WBSNode? WBSNode { get; set; }
        public List<Relationships.Relationship> Relationships { get; set; } = new();
        public List<Constraints.Constraint> Constraints { get; set; } = new();
        public List<Resources.ResourceAssignment> ResourceAssignments { get; set; } = new();
        public List<string> BOQReferences { get; set; } = new();
        public Calendars.Calendar? Calendar { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ActivityId))
                throw new InvalidOperationException("Activity ActivityId must not be empty.");
            if (Duration.HasValue && Duration.Value <= TimeSpan.Zero)
                throw new InvalidOperationException("Activity Duration must be > TimeSpan.Zero if set.");
            if (ProductivityRate < 0)
                throw new InvalidOperationException("Activity ProductivityRate must be >= 0.");
        }
    }
}
