using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using Newtonsoft.Json;

namespace Som3a_WPF_UI.Controls.Shell
{
    public partial class SidebarControl : UserControl
    {
        private bool _isHoverExpanded;
        private bool _isCollapsed;
        private readonly Dictionary<string, bool> _groupStates = new();
        private readonly string _stateFilePath;

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(ObservableCollection<NavigationDestination>),
                typeof(SidebarControl),
                new PropertyMetadata(null, OnItemsChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(NavigationDestination),
                typeof(SidebarControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.Register(
                nameof(NavigateCommand),
                typeof(ICommand),
                typeof(SidebarControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(
                nameof(IsCollapsed),
                typeof(bool),
                typeof(SidebarControl),
                new PropertyMetadata(false, OnIsCollapsedChanged));

        public ObservableCollection<NavigationDestination> Items
        {
            get => (ObservableCollection<NavigationDestination>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public NavigationDestination SelectedItem
        {
            get => (NavigationDestination)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public ICommand NavigateCommand
        {
            get => (ICommand)GetValue(NavigateCommandProperty);
            set => SetValue(NavigateCommandProperty, value);
        }

        public bool IsCollapsed
        {
            get => (bool)GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }

        public event SelectionChangedEventHandler SelectionChanged;

        public SidebarControl()
        {
            InitializeComponent();
            _stateFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Som3a", "sidebar-state.json");
            LoadGroupStates();
        }

        public void ToggleCollapse()
        {
            IsCollapsed = !IsCollapsed;
        }

        public void SetCollapsed(bool collapsed)
        {
            IsCollapsed = collapsed;
        }

        public void TemporarilyExpand()
        {
            if (IsCollapsed && !_isHoverExpanded)
            {
                _isHoverExpanded = true;
                SetCollapsed(false);
            }
        }

        public void TemporarilyCollapse()
        {
            if (_isHoverExpanded)
            {
                _isHoverExpanded = false;
                SetCollapsed(true);
            }
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SidebarControl)d;
            control.TryRebuildGroupedView();
        }

        private void TryRebuildGroupedView()
        {
            if (SidebarListBox == null || Items == null)
                return;
            if (!IsLoaded)
            {
                RoutedEventHandler handler = null;
                handler = (s, ev) =>
                {
                    Loaded -= handler;
                    RebuildGroupedView();
                };
                Loaded += handler;
                return;
            }
            RebuildGroupedView();
        }

        private void RebuildGroupedView()
        {
            var view = new ListCollectionView(Items)
            {
                Filter = dest =>
                {
                    if (dest is NavigationDestination nd)
                        return nd.IsVisible;
                    return true;
                }
            };
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NavigationDestination.Category)));
            view.SortDescriptions.Add(
                new System.ComponentModel.SortDescription(nameof(NavigationDestination.Category),
                    System.ComponentModel.ListSortDirection.Ascending));
            view.SortDescriptions.Add(
                new System.ComponentModel.SortDescription(nameof(NavigationDestination.Order),
                    System.ComponentModel.ListSortDirection.Ascending));

            SidebarListBox.ItemsSource = view;
            Dispatcher.BeginInvoke(new Action(RestoreExpanderStates), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private void LoadGroupStates()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    var json = File.ReadAllText(_stateFilePath);
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
                    if (loaded != null)
                    {
                        _groupStates.Clear();
                        foreach (var kvp in loaded)
                            _groupStates[kvp.Key] = kvp.Value;
                        return;
                    }
                }
            }
            catch { }
            _groupStates["Planning"] = false;
            _groupStates["Analysis"] = false;
            _groupStates["Excel"] = false;
            _groupStates["AI"] = false;
            _groupStates["Settings"] = false;
        }

        private void SaveGroupStates()
        {
            try
            {
                var dir = Path.GetDirectoryName(_stateFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var json = JsonConvert.SerializeObject(_groupStates, Formatting.Indented);
                File.WriteAllText(_stateFilePath, json);
            }
            catch { }
        }

        private void RestoreExpanderStates()
        {
            var expanders = FindVisualChildren<Expander>(SidebarListBox);
            foreach (var exp in expanders)
            {
                var header = exp.Header?.ToString();
                if (header != null && _groupStates.TryGetValue(header, out var isExpanded))
                {
                    exp.IsExpanded = isExpanded;
                }
                exp.Expanded -= OnExpanderToggled;
                exp.Collapsed -= OnExpanderToggled;
                exp.Expanded += OnExpanderToggled;
                exp.Collapsed += OnExpanderToggled;
            }
        }

        private void OnExpanderToggled(object sender, RoutedEventArgs e)
        {
            if (sender is Expander exp && exp.Header?.ToString() is string header)
            {
                _groupStates[header] = exp.IsExpanded;
                SaveGroupStates();
            }
        }

        private static List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var results = new List<T>();
            if (parent == null) return results;
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child is T typed)
                        results.Add(typed);
                    queue.Enqueue(child);
                }
            }
            return results;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is NavigationDestination selected)
            {
                SetCurrentValue(SelectedItemProperty, selected);
            }
            SelectionChanged?.Invoke(this, e);
        }

        private void OnGridMouseEnter(object sender, MouseEventArgs e)
        {
            TemporarilyExpand();
        }

        private void OnGridMouseLeave(object sender, MouseEventArgs e)
        {
            TemporarilyCollapse();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this) as DependencyObject;
            var expander = focused is Visual || focused is Visual3D ? FindVisualParent<Expander>(focused) : null;

            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                if (expander != null)
                {
                    expander.IsExpanded = e.Key == Key.Right;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                if (expander != null)
                {
                    expander.IsExpanded = !expander.IsExpanded;
                    e.Handled = true;
                }
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
                parent = VisualTreeHelper.GetParent(parent);
            return parent as T;
        }
    }
}
