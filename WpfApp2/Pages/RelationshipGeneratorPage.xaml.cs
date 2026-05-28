using System;
using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Pages
{
    public partial class RelationshipGeneratorPage : Page
    {
        private RelationshipGeneratorViewModel _vm;

        public RelationshipGeneratorPage()
        {
            InitializeComponent();
            _vm = App.Container.Resolve<RelationshipGeneratorViewModel>();
            DataContext = _vm;
            WireViewModel();
        }

        private void WireViewModel()
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.BusyChanged += OnBusyChanged;
            _vm.CanCancelChanged += OnCanCancelChanged;
            SyncFromViewModel();
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_vm.StatusText):
                    txtFooterStatus.Text = _vm.StatusText;
                    break;
                case nameof(_vm.GenerationStatus):
                    txtGenerationStatus.Text = _vm.GenerationStatus;
                    break;
                case nameof(_vm.IsBusy):
                    BusyOverlay.Visibility = _vm.IsBusy ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(_vm.ValidationSummary):
                    txtValidationSummary.Text = _vm.ValidationSummary ?? string.Empty;
                    break;
                case nameof(_vm.AnalysisSummary):
                    txtAnalysis.Text = _vm.AnalysisSummary ?? string.Empty;
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
            RelationshipGrid.SetValue(FrameworkElement.DataContextProperty, _vm);
            RelationshipGrid.LoadRelationships(_vm.Relationships);
            txtFooterStatus.Text = _vm.StatusText;
            txtGenerationStatus.Text = _vm.GenerationStatus;
            BusyOverlay.Visibility = _vm.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            btnCancel.Visibility = _vm.CanCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            _vm.GenerateCommand.Execute(null);
        }

        private void btnAcceptAll_Click(object sender, RoutedEventArgs e)
        {
            _vm.AcceptAllCommand.Execute(null);
        }

        private void btnRejectAll_Click(object sender, RoutedEventArgs e)
        {
            _vm.RejectAllCommand.Execute(null);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _vm.CancelCommand.Execute(null);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            txtExportStatus.Text = "Export functionality will be implemented in Phase 6.";
        }
    }
}
