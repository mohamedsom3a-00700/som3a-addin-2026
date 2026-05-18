using System;

namespace Som3a.Shared.Models.Primavera
{
    /// <summary>
    /// Data Transfer Object for Primavera Project
    /// </summary>
    public class ProjectDto
    {
        /// <summary>
        /// Unique project identifier
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Project code (unique identifier string)
        /// </summary>
        public string ProjectCode { get; set; }

        /// <summary>
        /// Project display name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Project creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Project current status (e.g., Active, Closed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Project description or notes
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Project manager name
        /// </summary>
        public string ProjectManager { get; set; }

        /// <summary>
        /// Planned project start date
        /// </summary>
        public DateTime? PlannedStartDate { get; set; }

        /// <summary>
        /// Planned project finish date
        /// </summary>
        public DateTime? PlannedFinishDate { get; set; }

        /// <summary>
        /// Actual project start date
        /// </summary>
        public DateTime? ActualStartDate { get; set; }

        /// <summary>
        /// Actual project finish date
        /// </summary>
        public DateTime? ActualFinishDate { get; set; }

        /// <summary>
        /// Project completion percentage
        /// </summary>
        public decimal CompletionPercentage { get; set; }

        public override string ToString()
        {
            return $"{ProjectCode} - {ProjectName}";
        }

        public override bool Equals(object obj)
        {
            if (obj is ProjectDto other)
            {
                return ProjectId == other.ProjectId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ProjectId.GetHashCode();
        }
    }
}
