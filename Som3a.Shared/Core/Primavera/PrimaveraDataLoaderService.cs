using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a.Shared.Models.Primavera;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Service for loading Primavera project data efficiently with progress tracking
    /// </summary>
    public class PrimaveraDataLoaderService : IPrimaveraDataLoaderService
    {
        private readonly IPrimaveraDbService _dbService;
        private CancellationTokenSource _cancellationTokenSource;

        public PrimaveraDataLoaderService(IPrimaveraDbService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        /// <summary>
        /// Loads all data for a single project
        /// </summary>
        public async Task<ProjectFullData> LoadProjectDataAsync(
            string connectionString,
            string databaseType,
            int projectId,
            IProgress<int> progress = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var projectData = new ProjectFullData();
                int totalSteps = 4; // Project + Activities + Relationships + Resources
                int completedSteps = 0;

                // Step 1: Load project info
                progress?.Report(0);
                projectData.Project = await _dbService.GetProjectByIdAsync(connectionString, databaseType, projectId);
                completedSteps++;
                progress?.Report((int)((completedSteps / (float)totalSteps) * 100));

                if (projectData.Project == null)
                    return projectData;

                // Step 2: Load activities
                projectData.Activities = await _dbService.GetActivitiesAsync(connectionString, databaseType, projectId);
                completedSteps++;
                progress?.Report((int)((completedSteps / (float)totalSteps) * 100));

                // Step 3: Load relationships
                projectData.Relationships = await _dbService.GetRelationshipsAsync(connectionString, databaseType, projectId);
                completedSteps++;
                progress?.Report((int)((completedSteps / (float)totalSteps) * 100));

                // Step 4: Load resources
                projectData.Resources = await _dbService.GetResourcesAsync(connectionString, databaseType, projectId);
                completedSteps++;
                progress?.Report(100);

                projectData.LoadedTime = DateTime.Now;
                return projectData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading project data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads data for multiple projects in parallel
        /// </summary>
        public async Task<List<ProjectFullData>> LoadMultipleProjectsAsync(
            string connectionString,
            string databaseType,
            List<int> projectIds,
            IProgress<int> progress = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (projectIds == null || projectIds.Count == 0)
                    return new List<ProjectFullData>();

                var projectDataList = new List<ProjectFullData>();
                int totalProjects = projectIds.Count;
                int completedProjects = 0;
                object lockObject = new object();

                // Load projects in parallel with a maximum degree of parallelism
                var loadingTasks = projectIds.Select(async projectId =>
                {
                    try
                    {
                        var projectProgress = new Progress<int>();
                        var projectData = await LoadProjectDataAsync(
                            connectionString,
                            databaseType,
                            projectId,
                            projectProgress);

                        lock (lockObject)
                        {
                            completedProjects++;
                            progress?.Report((int)((completedProjects / (float)totalProjects) * 100));
                            projectDataList.Add(projectData);
                        }

                        return projectData;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading project {projectId}: {ex.Message}");
                        return null;
                    }
                }).ToList();

                // Configure maximum parallelism (typically 3-4 concurrent loads)
                var maxDegreeOfParallelism = Math.Min(projectIds.Count, Environment.ProcessorCount);
                var tasks = loadingTasks.Take(maxDegreeOfParallelism).ToList();
                var remainingTasks = new Queue<Task<ProjectFullData>>(loadingTasks.Skip(maxDegreeOfParallelism));

                while (tasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(tasks);
                    tasks.Remove(completedTask);

                    if (remainingTasks.Count > 0)
                    {
                        tasks.Add(remainingTasks.Dequeue());
                    }
                }

                // Wait for all remaining tasks
                await Task.WhenAll(loadingTasks);

                return projectDataList.Where(p => p != null).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading multiple projects: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cancels any running load operation
        /// </summary>
        public void CancelLoading()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
