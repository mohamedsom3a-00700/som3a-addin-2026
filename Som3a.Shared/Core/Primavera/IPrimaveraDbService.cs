using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Som3a.Shared.Models.Primavera;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Interface for Primavera database connectivity and data retrieval
    /// </summary>
    public interface IPrimaveraDbService
    {
        /// <summary>
        /// Tests connection to Primavera database
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync(string connectionString, string databaseType);

        /// <summary>
        /// Retrieves list of all projects from Primavera database
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <returns>List of ProjectDto objects</returns>
        Task<List<ProjectDto>> GetProjectsAsync(string connectionString, string databaseType);

        /// <summary>
        /// Retrieves all activities/tasks for a specific project
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectId">Project ID to retrieve activities for</param>
        /// <returns>List of ActivityDto objects</returns>
        Task<List<ActivityDto>> GetActivitiesAsync(string connectionString, string databaseType, int projectId);

        /// <summary>
        /// Retrieves all task relationships/predecessors for a specific project
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectId">Project ID to retrieve relationships for</param>
        /// <returns>List of RelationshipDto objects</returns>
        Task<List<RelationshipDto>> GetRelationshipsAsync(string connectionString, string databaseType, int projectId);

        /// <summary>
        /// Retrieves all resource assignments for a specific project
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectId">Project ID to retrieve resources for</param>
        /// <returns>List of ResourceDto objects</returns>
        Task<List<ResourceDto>> GetResourcesAsync(string connectionString, string databaseType, int projectId);

        /// <summary>
        /// Retrieves a single project by ID
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectId">Project ID to retrieve</param>
        /// <returns>ProjectDto object or null if not found</returns>
        Task<ProjectDto> GetProjectByIdAsync(string connectionString, string databaseType, int projectId);
    }
}
