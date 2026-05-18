using System;

namespace Som3a.Shared.Models.Primavera
{
    /// <summary>
    /// Data Transfer Object for Primavera Activity/Task
    /// </summary>
    public class ActivityDto
    {
        /// <summary>
        /// Unique task identifier
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Task code (unique within project)
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// Task name or description
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Task status code (e.g., "Not Started", "In Progress", "Completed")
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Work Breakdown Structure ID
        /// </summary>
        public int WbsId { get; set; }

        /// <summary>
        /// Work Breakdown Structure full name/path
        /// </summary>
        public string WbsFullName { get; set; }

        /// <summary>
        /// Completion percentage (0-100)
        /// </summary>
        public decimal CompletePct { get; set; }

        /// <summary>
        /// Actual start date
        /// </summary>
        public DateTime? ActStartDate { get; set; }

        /// <summary>
        /// Actual end date
        /// </summary>
        public DateTime? ActEndDate { get; set; }

        /// <summary>
        /// Planned start date
        /// </summary>
        public DateTime? PlanStartDate { get; set; }

        /// <summary>
        /// Planned end date
        /// </summary>
        public DateTime? PlanEndDate { get; set; }

        /// <summary>
        /// Duration in days
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Task type (e.g., Task, Milestone)
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// Resource name assigned to task
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Remaining duration
        /// </summary>
        public int RemainingDuration { get; set; }

        /// <summary>
        /// Actual duration
        /// </summary>
        public int ActualDuration { get; set; }

        /// <summary>
        /// Free float days
        /// </summary>
        public int FreeFloat { get; set; }

        /// <summary>
        /// Total float days
        /// </summary>
        public int TotalFloat { get; set; }

        /// <summary>
        /// Project ID (foreign key reference)
        /// </summary>
        public int ProjectId { get; set; }

        public override string ToString()
        {
            return $"{TaskCode} - {TaskName}";
        }

        public override bool Equals(object obj)
        {
            if (obj is ActivityDto other)
            {
                return TaskId == other.TaskId && ProjectId == other.ProjectId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + TaskId.GetHashCode();
                hash = hash * 31 + ProjectId.GetHashCode();
                return hash;
            }
        }
    }
}
