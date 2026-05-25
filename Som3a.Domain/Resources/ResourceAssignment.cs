using Som3a.Domain.Activities;

namespace Som3a.Domain.Resources
{
    public class ResourceAssignment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public Resource Resource { get; set; } = null!;
        public Activity Activity { get; set; } = null!;
        public decimal Quantity { get; set; }

        public decimal Cost
        {
            get
            {
                if (Resource == null || Activity?.Duration == null)
                    return 0;
                var hours = (decimal)Activity.Duration.Value.TotalHours;
                if (hours <= 0) return 0;
                return Quantity * Resource.CostPerHour * hours;
            }
        }
    }
}
