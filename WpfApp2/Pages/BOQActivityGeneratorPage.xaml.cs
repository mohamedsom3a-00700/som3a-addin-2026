using System;
using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class BOQActivityGeneratorPage : Page
    {
        private BOQActivityGeneratorViewModel _vm;

        public BOQActivityGeneratorPage()
        {
            InitializeComponent();
            _vm = App.Container.Resolve<BOQActivityGeneratorViewModel>();
            DataContext = _vm;
            WireViewModel();
        }

        private void WireViewModel()
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.RequestClose += () => Som3a_WPF_UI.Services.NavigationService.Instance.GoBack();
            _vm.BusyChanged += OnBusyChanged;
            _vm.CanCancelChanged += OnCanCancelChanged;
            _vm.Activities.CollectionChanged += (_, _) => UpdateEmptyState();
            SyncFromViewModel();
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_vm.StatusText):
                    txtStatus.Text = _vm.StatusText;
                    break;
                case nameof(_vm.GenerationStatus):
                    txtGenerationStatus.Text = _vm.GenerationStatus;
                    break;
                case nameof(_vm.IsBusy):
                    BusyOverlay.Visibility = _vm.IsBusy ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        private void OnBusyChanged(bool isBusy)
        {
            BusyOverlay.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
            btnGenerate.IsEnabled = !isBusy;
        }

        private void OnCanCancelChanged(bool canCancel)
        {
            btnCancel.Visibility = canCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SyncFromViewModel()
        {
            dgBoqPreview.ItemsSource = _vm.BoqItems;
            dgActivities.ItemsSource = _vm.Activities;
            dgSequence.ItemsSource = _vm.SequenceOrder;
            dgDependencies.ItemsSource = _vm.Dependencies;
            txtStatus.Text = _vm.StatusText;
            BusyOverlay.Visibility = _vm.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            btnCancel.Visibility = _vm.CanCancel ? Visibility.Visible : Visibility.Collapsed;
            if (_vm.HasConsented)
                PrivacyOverlay.Visibility = Visibility.Collapsed;
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            var hasActivities = _vm.Activities.Count > 0;
            txtEmptyState.Visibility = hasActivities ? Visibility.Collapsed : Visibility.Visible;
            dgActivities.Visibility = hasActivities ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnLoadBoq_Click(object sender, RoutedEventArgs e)
        {
            _vm.LoadBoqCommand.Execute(null);
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            _vm.GenerateCommand.Execute(null);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _vm.CancelCommand.Execute(null);
        }

        private void dgActivities_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _vm.HandleCellEdit(e);
        }

        private void btnSequence_Click(object sender, RoutedEventArgs e)
        {
            _vm.SequenceCommand.Execute(null);
        }

        private void btnSuggestDeps_Click(object sender, RoutedEventArgs e)
        {
            _vm.SuggestDependenciesCommand.Execute(null);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            _vm.ConfirmOverwrite = true;
            _vm.ExportCommand.Execute(null);
        }

        private void btnConsent_Click(object sender, RoutedEventArgs e)
        {
            _vm.ConsentCommand.Execute(true);
            PrivacyOverlay.Visibility = Visibility.Collapsed;
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            _vm.ConsentCommand.Execute(false);
        }
    }
}
