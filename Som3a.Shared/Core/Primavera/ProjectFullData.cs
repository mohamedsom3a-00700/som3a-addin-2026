using System;
using System.Collections.Generic;
using Som3a.Shared.Models.Primavera;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Data Transfer Object containing all project data (Activities, Relationships, Resources)
    /// </summary>
    public class ProjectFullData
    {
        /// <summary>
        /// Project information
        /// </summary>
        public ProjectDto Project { get; set; }

        /// <summary>
        /// All activities/tasks for the project
        /// </summary>
        public List<ActivityDto> Activities { get; set; }

        /// <summary>
        /// All task relationships/predecessors for the project
        /// </summary>
        public List<RelationshipDto> Relationships { get; set; }

        /// <summary>
        /// All resource assignments for the project
        /// </summary>
        public List<ResourceDto> Resources { get; set; }

        /// <summary>
        /// Timestamp when data was loaded
        /// </summary>
        public DateTime LoadedTime { get; set; }

        /// <summary>
        /// Total records loaded
        /// </summary>
        public int TotalRecordsLoaded => 
            (Activities?.Count ?? 0) + (Relationships?.Count ?? 0) + (Resources?.Count ?? 0);

        public ProjectFullData()
        {
            Activities = new List<ActivityDto>();
            Relationships = new List<RelationshipDto>();
            Resources = new List<ResourceDto>();
            LoadedTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Project?.ProjectCode} - Loaded: {LoadedTime:g}";
        }
    }
}
