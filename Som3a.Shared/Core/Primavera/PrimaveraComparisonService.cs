using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Som3a.Shared.Models.Primavera;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Service for comparing Primavera project data efficiently
    /// Supports activities, relationships, and resources comparison
    /// </summary>
    public class PrimaveraComparisonService : IPrimaveraComparisonService
    {
        /// <summary>
        /// Default columns to compare for activities
        /// </summary>
        private static readonly string[] DefaultActivityColumns = new[]
        {
            nameof(ActivityDto.TaskCode),
            nameof(ActivityDto.TaskName),
            nameof(ActivityDto.StatusCode),
            nameof(ActivityDto.WbsFullName),
            nameof(ActivityDto.CompletePct),
            nameof(ActivityDto.ActStartDate),
            nameof(ActivityDto.ActEndDate),
            nameof(ActivityDto.Duration),
            nameof(ActivityDto.FreeFloat),
            nameof(ActivityDto.TotalFloat)
        };

        /// <summary>
        /// Default columns to compare for relationships
        /// </summary>
        private static readonly string[] DefaultRelationshipColumns = new[]
        {
            nameof(RelationshipDto.PredType),
            nameof(RelationshipDto.PredTaskStatusCode),
            nameof(RelationshipDto.TaskStatusCode),
            nameof(RelationshipDto.LagHrCnt),
            nameof(RelationshipDto.PredTaskWbsFullName),
            nameof(RelationshipDto.TaskWbsFullName)
        };

        /// <summary>
        /// Default columns to compare for resources
        /// </summary>
        private static readonly string[] DefaultResourceColumns = new[]
        {
            nameof(ResourceDto.RsrcName),
            nameof(ResourceDto.RsrcType),
            nameof(ResourceDto.TargetQty),
            nameof(ResourceDto.TargetCost),
            nameof(ResourceDto.ActualQty),
            nameof(ResourceDto.ActualCost)
        };

        /// <summary>
        /// Compares two complete projects
        /// </summary>
        public async Task<ComparisonResult> CompareProjectsAsync(
            ProjectFullData project1Data,
            ProjectFullData project2Data,
            string[] compareColumns = null)
        {
            if (project1Data == null || project2Data == null)
                throw new ArgumentNullException("Project data cannot be null");

            var result = new ComparisonResult
            {
                Project1Name = project1Data.Project?.ProjectName ?? "Project 1",
                Project2Name = project2Data.Project?.ProjectName ?? "Project 2"
            };

            // Compare activities
            result.ActivityDifferences = await CompareActivitiesAsync(
                project1Data.Activities,
                project2Data.Activities,
                compareColumns);

            // Compare relationships
            result.RelationshipDifferences = await CompareRelationshipsAsync(
                project1Data.Relationships,
                project2Data.Relationships);

            // Compare resources
            result.ResourceDifferences = await CompareResourcesAsync(
                project1Data.Resources,
                project2Data.Resources);

            // Calculate summary
            CalculateSummary(result);

            return result;
        }

        /// <summary>
        /// Compares activities from two projects
        /// </summary>
        public async Task<List<ActivityDiff>> CompareActivitiesAsync(
    List<ActivityDto> project1Activities,
    List<ActivityDto> project2Activities,
    string[] compareColumns = null)
        {
            return await Task.Run(() =>
            {
                var differences = new List<ActivityDiff>();

                var columnsToCompare =
                    compareColumns ?? DefaultActivityColumns;

                var project1Dict =
                    project1Activities?
                        .GroupBy(a => a.TaskId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<int, ActivityDto>();

                var project2Dict =
                    project2Activities?
                        .GroupBy(a => a.TaskId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<int, ActivityDto>();

                foreach (var activity2 in project2Dict.Values)
                {
                    if (!project1Dict.ContainsKey(activity2.TaskId))
                    {
                        differences.Add(new ActivityDiff
                        {
                            Type = DifferenceType.Added,
                            Project2Activity = activity2
                        });
                    }
                }

                foreach (var activity1 in project1Dict.Values)
                {
                    if (!project2Dict.ContainsKey(activity1.TaskId))
                    {
                        differences.Add(new ActivityDiff
                        {
                            Type = DifferenceType.Deleted,
                            Project1Activity = activity1
                        });
                    }
                }

                foreach (var activity1 in project1Dict.Values)
                {
                    if (project2Dict.TryGetValue(
                            activity1.TaskId,
                            out var activity2))
                    {
                        var changes =
                            CompareObjects(
                                activity1,
                                activity2,
                                columnsToCompare);

                        if (changes.Count > 0)
                        {
                            differences.Add(new ActivityDiff
                            {
                                Type = DifferenceType.Modified,
                                Project1Activity = activity1,
                                Project2Activity = activity2,
                                ChangedColumns = changes
                            });
                        }
                    }
                }

                return differences;
            });
        }

        /// <summary>
        /// Compares relationships from two projects
        /// </summary>
        public async Task<List<RelationshipDiff>> CompareRelationshipsAsync(
    List<RelationshipDto> project1Relationships,
    List<RelationshipDto> project2Relationships)
        {
            return await Task.Run(() =>
            {
                var differences = new List<RelationshipDiff>();

                var project1Dict =
                    project1Relationships?
                        .GroupBy(r =>
                            $"{r.PredTaskId}-" +
                            $"{r.TaskId}-" +
                            $"{r.PredType}-" +
                            $"{r.LagHrCnt}")
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<string, RelationshipDto>();

                var project2Dict =
                    project2Relationships?
                        .GroupBy(r =>
                            $"{r.PredTaskId}-" +
                            $"{r.TaskId}-" +
                            $"{r.PredType}-" +
                            $"{r.LagHrCnt}")
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<string, RelationshipDto>();

                foreach (var rel2 in project2Dict)
                {
                    if (!project1Dict.ContainsKey(rel2.Key))
                    {
                        differences.Add(new RelationshipDiff
                        {
                            Type = DifferenceType.Added,
                            Project2Relationship = rel2.Value
                        });
                    }
                }

                foreach (var rel1 in project1Dict)
                {
                    if (!project2Dict.ContainsKey(rel1.Key))
                    {
                        differences.Add(new RelationshipDiff
                        {
                            Type = DifferenceType.Deleted,
                            Project1Relationship = rel1.Value
                        });
                    }
                }

                foreach (var rel1 in project1Dict)
                {
                    if (project2Dict.TryGetValue(
                            rel1.Key,
                            out var rel2))
                    {
                        var changes =
                            CompareObjects(
                                rel1.Value,
                                rel2,
                                DefaultRelationshipColumns);

                        if (changes.Count > 0)
                        {
                            differences.Add(new RelationshipDiff
                            {
                                Type = DifferenceType.Modified,
                                Project1Relationship = rel1.Value,
                                Project2Relationship = rel2,
                                ChangedColumns = changes
                            });
                        }
                    }
                }

                return differences;
            });
        }

        /// <summary>
        /// Compares resources from two projects
        /// </summary>
        public async Task<List<ResourceDiff>> CompareResourcesAsync(
    List<ResourceDto> project1Resources,
    List<ResourceDto> project2Resources)
        {
            return await Task.Run(() =>
            {
                var differences = new List<ResourceDiff>();

                var project1Dict =
                    project1Resources?
                        .GroupBy(r =>
                            $"{r.RsrcId}-" +
                            $"{r.TaskId}-" +
                            $"{r.Role}")
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<string, ResourceDto>();

                var project2Dict =
                    project2Resources?
                        .GroupBy(r =>
                            $"{r.RsrcId}-" +
                            $"{r.TaskId}-" +
                            $"{r.Role}")
                        .ToDictionary(
                            g => g.Key,
                            g => g.First())
                    ?? new Dictionary<string, ResourceDto>();

                foreach (var res2 in project2Dict)
                {
                    if (!project1Dict.ContainsKey(res2.Key))
                    {
                        differences.Add(new ResourceDiff
                        {
                            Type = DifferenceType.Added,
                            Project2Resource = res2.Value
                        });
                    }
                }

                foreach (var res1 in project1Dict)
                {
                    if (!project2Dict.ContainsKey(res1.Key))
                    {
                        differences.Add(new ResourceDiff
                        {
                            Type = DifferenceType.Deleted,
                            Project1Resource = res1.Value
                        });
                    }
                }

                foreach (var res1 in project1Dict)
                {
                    if (project2Dict.TryGetValue(
                            res1.Key,
                            out var res2))
                    {
                        var changes =
                            CompareObjects(
                                res1.Value,
                                res2,
                                DefaultResourceColumns);

                        if (changes.Count > 0)
                        {
                            differences.Add(new ResourceDiff
                            {
                                Type = DifferenceType.Modified,
                                Project1Resource = res1.Value,
                                Project2Resource = res2,
                                ChangedColumns = changes
                            });
                        }
                    }
                }

                return differences;
            });
        }

        /// <summary>
        /// Generic method to compare two objects for specified columns
        /// </summary>
        private Dictionary<string, ValueChange> CompareObjects(
            object obj1,
            object obj2,
            string[] propertyNames)
        {
            var changes = new Dictionary<string, ValueChange>();

            if (obj1 == null || obj2 == null)
                return changes;

            var type = obj1.GetType();

            foreach (var propName in propertyNames)
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null)
                    continue;

                var value1 = prop.GetValue(obj1);
                var value2 = prop.GetValue(obj2);

                // Check if values are different
                if (!Equals(value1, value2))
                {
                    changes[propName] = new ValueChange
                    {
                        FieldName = propName,
                        OldValue = value1,
                        NewValue = value2
                    };
                }
            }

            return changes;
        }

        /// <summary>
        /// Calculates summary statistics for comparison
        /// </summary>
        private void CalculateSummary(ComparisonResult result)
        {
            result.Summary.ActivitiesAdded = result.ActivityDifferences.Count(d => d.Type == DifferenceType.Added);
            result.Summary.ActivitiesDeleted = result.ActivityDifferences.Count(d => d.Type == DifferenceType.Deleted);
            result.Summary.ActivitiesModified = result.ActivityDifferences.Count(d => d.Type == DifferenceType.Modified);

            result.Summary.RelationshipsAdded = result.RelationshipDifferences.Count(d => d.Type == DifferenceType.Added);
            result.Summary.RelationshipsDeleted = result.RelationshipDifferences.Count(d => d.Type == DifferenceType.Deleted);
            result.Summary.RelationshipsModified = result.RelationshipDifferences.Count(d => d.Type == DifferenceType.Modified);

            result.Summary.ResourcesAdded = result.ResourceDifferences.Count(d => d.Type == DifferenceType.Added);
            result.Summary.ResourcesDeleted = result.ResourceDifferences.Count(d => d.Type == DifferenceType.Deleted);
            result.Summary.ResourcesModified = result.ResourceDifferences.Count(d => d.Type == DifferenceType.Modified);
        }
    }
}
