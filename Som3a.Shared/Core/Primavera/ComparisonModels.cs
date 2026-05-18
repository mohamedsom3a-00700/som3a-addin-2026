using System;
using Som3a.Shared.Models.Primavera;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Enum representing the type of difference between two project versions
    /// </summary>
    public enum DifferenceType
    {
        /// <summary>
        /// Item exists in project 2 but not in project 1
        /// </summary>
        Added,

        /// <summary>
        /// Item exists in project 1 but not in project 2
        /// </summary>
        Deleted,

        /// <summary>
        /// Item exists in both projects but values are different
        /// </summary>
        Modified
    }

    /// <summary>
    /// Represents a change in a single field value
    /// </summary>
    public class ValueChange
    {
        /// <summary>
        /// Original value from project 1
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// New value from project 2
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Field name that changed
        /// </summary>
        public string FieldName { get; set; }

        public override string ToString()
        {
            return $"{FieldName}: {OldValue} → {NewValue}";
        }
    }

    /// <summary>
    /// Represents difference in activity/task data between two projects
    /// </summary>
    public class ActivityDiff
    {
        /// <summary>
        /// Type of difference
        /// </summary>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Activity data from project 1
        /// </summary>
        public ActivityDto Project1Activity { get; set; }

        /// <summary>
        /// Activity data from project 2
        /// </summary>
        public ActivityDto Project2Activity { get; set; }

        /// <summary>
        /// Dictionary of changed columns (field name -> value change)
        /// </summary>
        public System.Collections.Generic.Dictionary<string, ValueChange> ChangedColumns { get; set; }

        public ActivityDiff()
        {
            ChangedColumns = new System.Collections.Generic.Dictionary<string, ValueChange>();
        }

        public override string ToString()
        {
            var activity = Project1Activity ?? Project2Activity;
            return $"{Type}: {activity?.TaskCode} - {activity?.TaskName}";
        }
    }

    /// <summary>
    /// Represents difference in relationship/predecessor data between two projects
    /// </summary>
    public class RelationshipDiff
    {
        /// <summary>
        /// Type of difference
        /// </summary>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Relationship data from project 1
        /// </summary>
        public RelationshipDto Project1Relationship { get; set; }

        /// <summary>
        /// Relationship data from project 2
        /// </summary>
        public RelationshipDto Project2Relationship { get; set; }

        /// <summary>
        /// Dictionary of changed columns
        /// </summary>
        public System.Collections.Generic.Dictionary<string, ValueChange> ChangedColumns { get; set; }

        public RelationshipDiff()
        {
            ChangedColumns = new System.Collections.Generic.Dictionary<string, ValueChange>();
        }

        public override string ToString()
        {
            var rel = Project1Relationship ?? Project2Relationship;
            return $"{Type}: {rel?.PredTaskCode} ({rel?.PredType}) {rel?.TaskCode}";
        }
    }

    /// <summary>
    /// Represents difference in resource assignment data between two projects
    /// </summary>
    public class ResourceDiff
    {
        /// <summary>
        /// Type of difference
        /// </summary>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Resource data from project 1
        /// </summary>
        public ResourceDto Project1Resource { get; set; }

        /// <summary>
        /// Resource data from project 2
        /// </summary>
        public ResourceDto Project2Resource { get; set; }

        /// <summary>
        /// Dictionary of changed columns
        /// </summary>
        public System.Collections.Generic.Dictionary<string, ValueChange> ChangedColumns { get; set; }

        public ResourceDiff()
        {
            ChangedColumns = new System.Collections.Generic.Dictionary<string, ValueChange>();
        }

        public override string ToString()
        {
            var resource = Project1Resource ?? Project2Resource;
            return $"{Type}: {resource?.RsrcName} - {resource?.TaskCode}";
        }
    }

    /// <summary>
    /// Complete comparison result containing all differences
    /// </summary>
    public class ComparisonResult
    {
        /// <summary>
        /// Activity differences
        /// </summary>
        public System.Collections.Generic.List<ActivityDiff> ActivityDifferences { get; set; }

        /// <summary>
        /// Relationship differences
        /// </summary>
        public System.Collections.Generic.List<RelationshipDiff> RelationshipDifferences { get; set; }

        /// <summary>
        /// Resource differences
        /// </summary>
        public System.Collections.Generic.List<ResourceDiff> ResourceDifferences { get; set; }

        /// <summary>
        /// Timestamp of comparison
        /// </summary>
        public DateTime ComparisonTime { get; set; }

        /// <summary>
        /// Summary statistics of comparison
        /// </summary>
        public ComparisonSummary Summary { get; set; }

        /// <summary>
        /// First project name
        /// </summary>
        public string Project1Name { get; set; }

        /// <summary>
        /// Second project name
        /// </summary>
        public string Project2Name { get; set; }

        public ComparisonResult()
        {
            ActivityDifferences = new System.Collections.Generic.List<ActivityDiff>();
            RelationshipDifferences = new System.Collections.Generic.List<RelationshipDiff>();
            ResourceDifferences = new System.Collections.Generic.List<ResourceDiff>();
            ComparisonTime = DateTime.Now;
            Summary = new ComparisonSummary();
        }

        public override string ToString()
        {
            return $"Comparison: {Project1Name} vs {Project2Name} - {ComparisonTime:g}";
        }
    }

    /// <summary>
    /// Summary statistics for comparison
    /// </summary>
    public class ComparisonSummary
    {
        public int ActivitiesAdded { get; set; }
        public int ActivitiesDeleted { get; set; }
        public int ActivitiesModified { get; set; }

        public int RelationshipsAdded { get; set; }
        public int RelationshipsDeleted { get; set; }
        public int RelationshipsModified { get; set; }

        public int ResourcesAdded { get; set; }
        public int ResourcesDeleted { get; set; }
        public int ResourcesModified { get; set; }

        public int TotalDifferences => 
            ActivitiesAdded + ActivitiesDeleted + ActivitiesModified +
            RelationshipsAdded + RelationshipsDeleted + RelationshipsModified +
            ResourcesAdded + ResourcesDeleted + ResourcesModified;

        public override string ToString()
        {
            return $"Activities: +{ActivitiesAdded}/-{ActivitiesDeleted}/~{ActivitiesModified} | " +
                   $"Relationships: +{RelationshipsAdded}/-{RelationshipsDeleted}/~{RelationshipsModified} | " +
                   $"Resources: +{ResourcesAdded}/-{ResourcesDeleted}/~{ResourcesModified}";
        }
    }
}
