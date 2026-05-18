using System;

namespace Som3a.Shared.Models.Primavera
{
    /// <summary>
    /// Data Transfer Object for Primavera Resource Assignment/Allocation
    /// </summary>
    public class ResourceDto
    {
        /// <summary>
        /// Unique resource ID
        /// </summary>
        public int RsrcId { get; set; }

        /// <summary>
        /// Task ID to which resource is assigned
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Task status code
        /// </summary>
        public string TaskStatusCode { get; set; }

        /// <summary>
        /// Resource name
        /// </summary>
        public string RsrcName { get; set; }

        /// <summary>
        /// Task name
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Task code
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// Resource type (e.g., Labor, Material, Equipment)
        /// </summary>
        public string RsrcType { get; set; }

        /// <summary>
        /// Target quantity for resource
        /// </summary>
        public decimal TargetQty { get; set; }

        /// <summary>
        /// Target cost for resource
        /// </summary>
        public decimal TargetCost { get; set; }

        /// <summary>
        /// Actual quantity used
        /// </summary>
        public decimal ActualQty { get; set; }

        /// <summary>
        /// Actual cost incurred
        /// </summary>
        public decimal ActualCost { get; set; }

        /// <summary>
        /// Remaining quantity to be used
        /// </summary>
        public decimal RemainingQty { get; set; }

        /// <summary>
        /// Resource role or responsibility
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Project ID (foreign key reference)
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Units of measurement
        /// </summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Cost per unit
        /// </summary>
        public decimal CostPerUnit { get; set; }

        /// <summary>
        /// Resource calendar ID
        /// </summary>
        public int? CalendarId { get; set; }

        public override string ToString()
        {
            return $"{RsrcName} - {TaskCode}";
        }

        public override bool Equals(object obj)
        {
            if (obj is ResourceDto other)
            {
                return RsrcId == other.RsrcId 
                    && TaskId == other.TaskId 
                    && ProjectId == other.ProjectId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + RsrcId.GetHashCode();
                hash = hash * 31 + TaskId.GetHashCode();
                hash = hash * 31 + ProjectId.GetHashCode();
                return hash;
            }
        }
    }
}
