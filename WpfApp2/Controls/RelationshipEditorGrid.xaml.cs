using Som3a_WPF_UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Controls
{
    public partial class RelationshipEditorGrid : UserControl
    {
        public ObservableCollection<Relationship> Relationships { get; } = new();
        public System.Collections.Generic.List<RelationshipType> TypeValues { get; } = new()
        {
            RelationshipType.FS,
            RelationshipType.SS,
            RelationshipType.FF,
            RelationshipType.SF
        };

        public event Action? RelationshipsChanged;

        public RelationshipEditorGrid()
        {
            InitializeComponent();
            dgRelationships.ItemsSource = Relationships;
            Relationships.CollectionChanged += OnRelationshipsChanged;
            UpdateSummary();
        }

        public void LoadRelationships(System.Collections.Generic.IEnumerable<Relationship> relationships)
        {
            Relationships.Clear();
            foreach (var r in relationships)
                Relationships.Add(r);
        }

        private void OnRelationshipsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSummary();
            RelationshipsChanged?.Invoke();
        }

        private void UpdateSummary()
        {
            var total = Relationships.Count;
            var accepted = Relationships.Count(r => r.IsAccepted);
            txtSummary.Text = $"{total} relationship(s), {accepted} accepted";
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            Relationships.Add(new Relationship
            {
                PredecessorId = string.Empty,
                SuccessorId = string.Empty,
                Type = RelationshipType.FS,
                IsAccepted = false,
                Confidence = RelationshipConfidence.Medium
            });
        }

        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgRelationships.SelectedItems.Cast<Relationship>().ToList();
            foreach (var r in selected)
                Relationships.Remove(r);
        }

        public string? PredecessorId
        {
            get => dgRelationships.CurrentColumn?.Header.ToString() == "Predecessor"
                ? (dgRelationships.CurrentItem as Relationship)?.PredecessorId
                : null;
        }
    }
}
