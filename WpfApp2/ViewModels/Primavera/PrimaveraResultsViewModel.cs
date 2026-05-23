using Som3a.Shared.Core.Primavera;
using Som3a.Shared.Models.Primavera;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Som3a_WPF_UI.ViewModels.Primavera
{
    public class PrimaveraResultsViewModel
    {
        public PrimaveraResultsViewModel(
            IServiceContainer container,
            ComparisonResult result)
        {
            Result = result ?? new ComparisonResult();
            ActivityRows = new ObservableCollection<ResultRow>(
                Result.ActivityDifferences.Select(ToActivityRow));
            RelationshipRows = new ObservableCollection<ResultRow>(
                Result.RelationshipDifferences.Select(ToRelationshipRow));
            ResourceRows = new ObservableCollection<ResultRow>(
                Result.ResourceDifferences.Select(ToResourceRow));
        }

        public ComparisonResult Result { get; }

        public ObservableCollection<ResultRow> ActivityRows { get; }

        public ObservableCollection<ResultRow> RelationshipRows { get; }

        public ObservableCollection<ResultRow> ResourceRows { get; }

        private static ResultRow ToActivityRow(ActivityDiff diff)
        {
            var oldActivity = diff.Project1Activity;
            var newActivity = diff.Project2Activity;

            return new ResultRow
            {
                DifferenceType = diff.Type,
                Code = oldActivity?.TaskCode ?? newActivity?.TaskCode ?? string.Empty,
                Name = oldActivity?.TaskName ?? newActivity?.TaskName ?? string.Empty,
                OldValues = FormatActivity(oldActivity),
                NewValues = FormatActivity(newActivity),
                ChangedColumns = FormatChanges(diff.ChangedColumns)
            };
        }

        private static ResultRow ToRelationshipRow(RelationshipDiff diff)
        {
            var oldRelationship = diff.Project1Relationship;
            var newRelationship = diff.Project2Relationship;
            var relationship = oldRelationship ?? newRelationship;

            return new ResultRow
            {
                DifferenceType = diff.Type,
                Code = relationship == null ? string.Empty : $"{relationship.PredTaskCode} -> {relationship.TaskCode}",
                Name = relationship?.PredType ?? string.Empty,
                OldValues = FormatRelationship(oldRelationship),
                NewValues = FormatRelationship(newRelationship),
                ChangedColumns = FormatChanges(diff.ChangedColumns)
            };
        }

        private static ResultRow ToResourceRow(ResourceDiff diff)
        {
            var oldResource = diff.Project1Resource;
            var newResource = diff.Project2Resource;
            var resource = oldResource ?? newResource;

            return new ResultRow
            {
                DifferenceType = diff.Type,
                Code = resource?.TaskCode ?? string.Empty,
                Name = resource?.RsrcName ?? string.Empty,
                OldValues = FormatResource(oldResource),
                NewValues = FormatResource(newResource),
                ChangedColumns = FormatChanges(diff.ChangedColumns)
            };
        }

        private static string FormatActivity(ActivityDto? activity)
        {
            if (activity == null)
                return string.Empty;

            return $"{activity.TaskCode} | {activity.TaskName} | {activity.StatusCode} | {activity.CompletePct}%";
        }

        private static string FormatRelationship(RelationshipDto? relationship)
        {
            if (relationship == null)
                return string.Empty;

            return $"{relationship.PredTaskCode} {relationship.PredType} {relationship.TaskCode} | Lag {relationship.LagHrCnt} {relationship.LagType}";
        }

        private static string FormatResource(ResourceDto? resource)
        {
            if (resource == null)
                return string.Empty;

            return $"{resource.RsrcName} | {resource.TaskCode} | Qty {resource.TargetQty} | Cost {resource.TargetCost}";
        }

        private static string FormatChanges(IDictionary<string, ValueChange> changes)
        {
            if (changes == null || changes.Count == 0)
                return string.Empty;

            return string.Join(
                Environment.NewLine,
                changes.Values.Select(change => $"{change.FieldName}: {change.OldValue} -> {change.NewValue}"));
        }
    }

    public class ResultRow
    {
        public DifferenceType DifferenceType { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string OldValues { get; set; } = string.Empty;

        public string NewValues { get; set; } = string.Empty;

        public string ChangedColumns { get; set; } = string.Empty;
    }
}
