namespace Som3a.Domain.Activities
{
    public sealed class ActivityId
    {
        public string Value { get; }

        public ActivityId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new System.ArgumentException("ActivityId value must not be empty.", nameof(value));
            Value = value;
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj) =>
            obj is ActivityId other && string.Equals(Value, other.Value, System.StringComparison.Ordinal);

        public override int GetHashCode() => Value.GetHashCode(System.StringComparison.Ordinal);

        public static bool operator ==(ActivityId? left, ActivityId? right) =>
            left?.Value == right?.Value;

        public static bool operator !=(ActivityId? left, ActivityId? right) =>
            !(left?.Value == right?.Value);
    }
}
