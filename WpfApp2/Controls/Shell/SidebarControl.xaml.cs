using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Som3a_WPF_UI.Controls.Shell
{
    public partial class SidebarControl : UserControl
    {
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

        public event SelectionChangedEventHandler SelectionChanged;

        public SidebarControl()
        {
            InitializeComponent();
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidebarControl control && e.NewValue is ObservableCollection<NavigationDestination> items)
            {
                control.SidebarListBox.ItemsSource = items;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = SidebarListBox.SelectedItem as NavigationDestination;
            if (selected != null)
            {
                SetCurrentValue(SelectedItemProperty, selected);
                if (NavigateCommand?.CanExecute(selected.Key) == true)
                    NavigateCommand.Execute(selected.Key);
            }
            SelectionChanged?.Invoke(this, e);
        }
    }
}
