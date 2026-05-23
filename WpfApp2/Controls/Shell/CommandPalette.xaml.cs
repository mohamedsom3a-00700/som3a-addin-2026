using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Som3a_WPF_UI.ViewModels;

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

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private readonly CommandPaletteViewModel _viewModel;

        public CommandPalette()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<CommandPaletteViewModel>();
            DataContext = _viewModel;
            ResultsListBox.ItemsSource = _viewModel.Results;

            _viewModel.RequestClose += () => IsOpen = false;
        }

        public void Open()
        {
            IsOpen = true;
            _viewModel.Open();
            SearchTextBox?.Focus();
            SearchTextBox?.SelectAll();
        }

        public void Close()
        {
            IsOpen = false;
            SearchTextBox.Text = string.Empty;
            _viewModel.Results.Clear();
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommandPalette palette && (bool)e.NewValue)
            {
                palette.SearchTextBox?.Focus();
                palette.SearchTextBox?.SelectAll();
                palette._viewModel.Open();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = SearchTextBox.Text;
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    _viewModel.SelectNextCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Up:
                    _viewModel.SelectPreviousCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    _viewModel.ExecuteCommand.Execute(null);
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
            _viewModel.ExecuteCommand.Execute(null);
        }
    }
}
