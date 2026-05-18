using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Interface for comparing Primavera project data
    /// </summary>
    public interface IPrimaveraComparisonService
    {
        /// <summary>
        /// Compares two projects and identifies all differences
        /// </summary>
        /// <param name="project1Data">First project data</param>
        /// <param name="project2Data">Second project data</param>
        /// <param name="compareColumns">Optional array of specific columns to compare (null = all)</param>
        /// <returns>ComparisonResult with all differences</returns>
        Task<ComparisonResult> CompareProjectsAsync(
            ProjectFullData project1Data,
            ProjectFullData project2Data,
            string[] compareColumns = null);

        /// <summary>
        /// Compares activities from two projects
        /// </summary>
        /// <param name="project1Activities">Activities from project 1</param>
        /// <param name="project2Activities">Activities from project 2</param>
        /// <param name="compareColumns">Columns to compare</param>
        /// <returns>List of activity differences</returns>
        Task<List<ActivityDiff>> CompareActivitiesAsync(
            List<Som3a.Shared.Models.Primavera.ActivityDto> project1Activities,
            List<Som3a.Shared.Models.Primavera.ActivityDto> project2Activities,
            string[] compareColumns = null);

        /// <summary>
        /// Compares relationships from two projects
        /// </summary>
        /// <param name="project1Relationships">Relationships from project 1</param>
        /// <param name="project2Relationships">Relationships from project 2</param>
        /// <returns>List of relationship differences</returns>
        Task<List<RelationshipDiff>> CompareRelationshipsAsync(
            List<Som3a.Shared.Models.Primavera.RelationshipDto> project1Relationships,
            List<Som3a.Shared.Models.Primavera.RelationshipDto> project2Relationships);

        /// <summary>
        /// Compares resources from two projects
        /// </summary>
        /// <param name="project1Resources">Resources from project 1</param>
        /// <param name="project2Resources">Resources from project 2</param>
        /// <returns>List of resource differences</returns>
        Task<List<ResourceDiff>> CompareResourcesAsync(
            List<Som3a.Shared.Models.Primavera.ResourceDto> project1Resources,
            List<Som3a.Shared.Models.Primavera.ResourceDto> project2Resources);
    }
}
