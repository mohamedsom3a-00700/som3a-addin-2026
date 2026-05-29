using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class ShellRTLManager
    {
        private static readonly Lazy<ShellRTLManager> _instance =
            new Lazy<ShellRTLManager>(() => new ShellRTLManager());

        public static ShellRTLManager Instance => _instance.Value;

        private readonly Dictionary<string, FrameworkElement> _registeredElements = new();
        private bool _isRTL;

        public bool IsRTL => _isRTL;

        public event EventHandler<bool> LayoutChanged;

        public void ApplyLayout(bool isRTL)
        {
            if (_isRTL == isRTL)
                return;

            _isRTL = isRTL;

            foreach (var kvp in _registeredElements)
            {
                ApplyFlowDirection(kvp.Value, isRTL);
            }

            LayoutChanged?.Invoke(this, isRTL);
        }

        public void RegisterFlowElement(FrameworkElement element, string elementId)
        {
            if (element == null || string.IsNullOrEmpty(elementId))
                return;

            _registeredElements[elementId] = element;

            if (_isRTL)
            {
                ApplyFlowDirection(element, true);
            }
        }

        public void UnregisterFlowElement(string elementId)
        {
            if (!string.IsNullOrEmpty(elementId) && _registeredElements.ContainsKey(elementId))
            {
                var element = _registeredElements[elementId];
                if (element != null && !element.IsLoaded)
                {
                    element.FlowDirection = FlowDirection.LeftToRight;
                }
                _registeredElements.Remove(elementId);
            }
        }

        public void ApplyRTLToShellGrid(Grid rootGrid, bool isRTL)
        {
            if (rootGrid == null)
                return;

            rootGrid.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            if (rootGrid.ColumnDefinitions.Count >= 2)
            {
                var sidebarCol = rootGrid.ColumnDefinitions[0];
                var contentCol = rootGrid.ColumnDefinitions[1];

                if (isRTL)
                {
                    sidebarCol.Width = rootGrid.ColumnDefinitions[0].Width;
                    Grid.SetColumn(rootGrid.Children[0] as FrameworkElement, 0);
                }
            }
        }

        public void MirrorSidebar(SidebarControl sidebar, bool isRTL)
        {
            if (sidebar == null)
                return;

            sidebar.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public void EnsureChildControlsFlowDirection(DependencyObject parent, bool isRTL)
        {
            if (parent == null)
                return;

            var direction = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current is FrameworkElement fe && fe.FlowDirection != direction)
                {
                    if (fe is DataGrid || fe is ListView || fe is ListBox ||
                        fe is ComboBox || fe is Menu || fe is ContextMenu)
                    {
                        fe.FlowDirection = direction;
                    }
                }

                var count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    queue.Enqueue(child);
                }
            }
        }

        private static void ApplyFlowDirection(FrameworkElement element, bool isRTL)
        {
            if (element == null)
                return;

            element.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
    }
}
