using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Interface for Primavera project data loading
    /// </summary>
    public interface IPrimaveraDataLoaderService
    {
        /// <summary>
        /// Loads all data for a single project asynchronously
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectId">Project ID to load</param>
        /// <param name="progress">Optional progress reporter (0-100)</param>
        /// <returns>ProjectFullData with all project information</returns>
        Task<ProjectFullData> LoadProjectDataAsync(
            string connectionString,
            string databaseType,
            int projectId,
            IProgress<int> progress = null);

        /// <summary>
        /// Loads data for multiple projects in parallel
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="databaseType">Database type: "SQLite", "Oracle", or "SqlServer"</param>
        /// <param name="projectIds">List of project IDs to load</param>
        /// <param name="progress">Optional progress reporter (0-100)</param>
        /// <returns>List of ProjectFullData objects</returns>
        Task<List<ProjectFullData>> LoadMultipleProjectsAsync(
            string connectionString,
            string databaseType,
            List<int> projectIds,
            IProgress<int> progress = null);

        /// <summary>
        /// Cancels any running load operation
        /// </summary>
        void CancelLoading();
    }
}
