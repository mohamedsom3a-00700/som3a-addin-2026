using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;

namespace Som3a_WPF_UI.Controls.Shell
{
    public partial class SidebarControl : UserControl
    {
        private bool _isHoverExpanded;
        private bool _isCollapsed;

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
        }

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
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
