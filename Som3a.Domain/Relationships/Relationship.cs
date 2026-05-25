using Som3a.Domain.Activities;

namespace Som3a.Domain.Relationships
{
    public enum RelationshipType
    {
        FS,
        SS,
        FF,
        SF
    }

    public enum ValidationStatus
    {
        Valid,
        Warning,
        Error
    }

    public class Relationship
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public Activity Predecessor { get; set; } = null!;
        public Activity Successor { get; set; } = null!;
        public RelationshipType Type { get; set; } = RelationshipType.FS;
        public TimeSpan Lag { get; set; }
        public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Valid;
        public string? ValidationMessage { get; set; }

        public void Validate()
        {
            if (Predecessor == null || Successor == null)
                throw new InvalidOperationException("Relationship must have both predecessor and successor.");
            if (Predecessor.Id == Successor.Id)
                throw new InvalidOperationException("Predecessor and successor cannot be the same activity.");
        }
    }
}
