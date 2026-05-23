using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Controls.Shell
{
    public partial class CommandPalette : UserControl
    {
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(CommandPalette),
                new PropertyMetadata(false, OnIsOpenChanged));

        public static readonly DependencyProperty DestinationsProperty =
            DependencyProperty.Register(
                nameof(Destinations),
                typeof(ObservableCollection<NavigationDestination>),
                typeof(CommandPalette),
                new PropertyMetadata(null));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public ObservableCollection<NavigationDestination> Destinations
        {
            get => (ObservableCollection<NavigationDestination>)GetValue(DestinationsProperty);
            set => SetValue(DestinationsProperty, value);
        }

        private readonly ObservableCollection<NavigationDestination> _filteredResults =
            new ObservableCollection<NavigationDestination>();

        public CommandPalette()
        {
            InitializeComponent();
            ResultsListBox.ItemsSource = _filteredResults;
        }

        public void Open()
        {
            IsOpen = true;
            SearchTextBox?.Focus();
            SearchTextBox?.SelectAll();
            UpdateResults(null);
        }

        public void Close()
        {
            IsOpen = false;
            SearchTextBox.Text = string.Empty;
            _filteredResults.Clear();
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommandPalette palette && (bool)e.NewValue)
            {
                palette.SearchTextBox?.Focus();
                palette.SearchTextBox?.SelectAll();
                palette.UpdateResults(null);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateResults(SearchTextBox.Text);
        }

        private void UpdateResults(string query)
        {
            _filteredResults.Clear();

            var source = Destinations;
            if (source == null) return;

            var results = string.IsNullOrWhiteSpace(query)
                ? source.ToList()
                : source.Where(d =>
                    d.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    d.Key.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            foreach (var item in results)
            {
                _filteredResults.Add(item);
            }

            if (_filteredResults.Count > 0)
                ResultsListBox.SelectedIndex = 0;
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (ResultsListBox.SelectedIndex < _filteredResults.Count - 1)
                        ResultsListBox.SelectedIndex++;
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (ResultsListBox.SelectedIndex > 0)
                        ResultsListBox.SelectedIndex--;
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (ResultsListBox.SelectedItem is NavigationDestination selected)
                        ExecuteNavigation(selected);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    Close();
                    e.Handled = true;
                    break;
            }
        }

        private void OnResultDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is NavigationDestination selected)
                ExecuteNavigation(selected);
        }

        private void ExecuteNavigation(NavigationDestination destination)
        {
            try
            {
                NavigationService.Instance.NavigateTo(destination.Key);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Navigation to '{destination.Label}' failed: {ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
