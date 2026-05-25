using Som3a.Domain.Activities;

namespace Som3a.Domain.Constraints
{
    public enum ConstraintType
    {
        StartOn,
        FinishOn,
        MandatoryStart,
        MandatoryFinish,
        StartOnOrAfter,
        FinishOnOrBefore
    }

    public class Constraint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public Activity Activity { get; set; } = null!;
        public ConstraintType ConstraintType { get; set; }
        public DateTime ConstraintDate { get; set; }
        public int FloatValue { get; set; }

        public void Validate()
        {
            if (Activity == null)
                throw new ArgumentNullException(nameof(Activity), "Constraint Activity must not be null.");
            if (!Enum.IsDefined(typeof(ConstraintType), ConstraintType))
                throw new ArgumentException(
                    $"Invalid ConstraintType value: {ConstraintType}.", nameof(ConstraintType));
            if (ConstraintDate == default)
                throw new ArgumentException(
                    "ConstraintDate must not be DateTime.MinValue.", nameof(ConstraintDate));
        }
    }
}
