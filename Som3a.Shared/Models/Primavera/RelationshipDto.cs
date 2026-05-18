using System;

namespace Som3a.Shared.Models.Primavera
{
    /// <summary>
    /// Data Transfer Object for Primavera Task Relationship/Predecessor
    /// </summary>
    public class RelationshipDto
    {
        /// <summary>
        /// Unique relationship identifier
        /// </summary>
        public int TaskPredId { get; set; }

        /// <summary>
        /// Predecessor task ID
        /// </summary>
        public int PredTaskId { get; set; }

        /// <summary>
        /// Successor task ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Relationship type (FS=Finish-Start, SS=Start-Start, FF=Finish-Finish, SF=Start-Finish)
        /// </summary>
        public string PredType { get; set; }

        /// <summary>
        /// Predecessor task status code
        /// </summary>
        public string PredTaskStatusCode { get; set; }

        /// <summary>
        /// Successor task status code
        /// </summary>
        public string TaskStatusCode { get; set; }

        /// <summary>
        /// Predecessor task WBS full name
        /// </summary>
        public string PredTaskWbsFullName { get; set; }

        /// <summary>
        /// Successor task WBS full name
        /// </summary>
        public string TaskWbsFullName { get; set; }

        /// <summary>
        /// Lag hours count (negative value = lead)
        /// </summary>
        public decimal LagHrCnt { get; set; }

        /// <summary>
        /// Predecessor task name
        /// </summary>
        public string PredTaskName { get; set; }

        /// <summary>
        /// Successor task name
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Predecessor task code
        /// </summary>
        public string PredTaskCode { get; set; }

        /// <summary>
        /// Successor task code
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// Project ID (foreign key reference)
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Lag type (e.g., Days, Hours)
        /// </summary>
        public string LagType { get; set; }

        public override string ToString()
        {
            return $"{PredTaskCode} ({PredType}) {TaskCode}";
        }

        public override bool Equals(object obj)
        {
            if (obj is RelationshipDto other)
            {
                return PredTaskId == other.PredTaskId 
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
                hash = hash * 31 + PredTaskId.GetHashCode();
                hash = hash * 31 + TaskId.GetHashCode();
                hash = hash * 31 + ProjectId.GetHashCode();
                return hash;
            }
        }
    }
}
