using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a.Bridge;
using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class DurationEstimatorPageViewModel : ViewModelBase
    {
        private readonly IDurationEstimatorBridge _bridge;
        private CancellationTokenSource _searchCts;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private decimal _productivityRate = 10m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private int _crewSize = 2;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private decimal _hoursPerDay = 8m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private decimal _quantity;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private bool _isVarianceMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private decimal _optimisticRate = 12m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCalculate))]
        private decimal _pessimisticRate = 7m;

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private string _selectedCategory = "All";

        [ObservableProperty]
        private BenchmarkDto _selectedBenchmark;

        partial void OnIsBusyChanged(bool value)
        {
            CalculateCommand.NotifyCanExecuteChanged();
            ExportExcelCommand.NotifyCanExecuteChanged();
        }

        partial void OnProductivityRateChanged(decimal value) => OnRateChanged();
        partial void OnCrewSizeChanged(int value) => OnRateChanged();
        partial void OnHoursPerDayChanged(decimal value) => OnRateChanged();
        partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(CanCalculate));
        partial void OnIsVarianceModeChanged(bool value) => OnPropertyChanged(nameof(CanCalculate));
        partial void OnOptimisticRateChanged(decimal value) => OnRateChanged();
        partial void OnPessimisticRateChanged(decimal value) => OnRateChanged();

        partial void OnSelectedCategoryChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            _ = SearchBenchmarksAsync(token);
        }

        public ObservableCollection<DurationEstimateItem> Estimates { get; } = new ObservableCollection<DurationEstimateItem>();
        public ObservableCollection<BenchmarkDto> Benchmarks { get; } = new ObservableCollection<BenchmarkDto>();
        public int BenchmarkCount { get { return Benchmarks.Count; } }
        public int EstimateCount { get { return Estimates.Count; } }
        public List<string> Categories { get; }

        public bool CanCalculate
        {
            get { return !IsBusy && Quantity > 0 && ProductivityRate > 0 && CrewSize >= 1 && HoursPerDay > 0; }
        }

        public bool CanExport
        {
            get { return !IsBusy && Estimates.Count > 0; }
        }

        public event Action RequestClose;

        public DurationEstimatorPageViewModel(IDurationEstimatorBridge bridge)
        {
            _bridge = bridge;

            Categories = new List<string>
            {
                "All", "concrete", "steel", "masonry", "mep",
                "finishes", "earthwork", "roofing", "glazing", "flooring"
            };
            _selectedCategory = "All";
            var t = LoadBenchmarksAsync();
        }

        private async Task LoadBenchmarksAsync()
        {
            try
            {
                var response = await _bridge.SearchBenchmarksAsync(new BenchmarkSearchRequest());
                if (response.IsSuccess)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Benchmarks.Clear();
                        foreach (var b in response.Benchmarks)
                            Benchmarks.Add(b);
                    });
                }
            }
            catch
            {
            }
        }

        [RelayCommand]
        private async Task SearchBenchmarks()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            await SearchBenchmarksAsync(_searchCts.Token);
        }

        private async Task SearchBenchmarksAsync(CancellationToken ct = default)
        {
            IsBusy = true;
            try
            {
                ct.ThrowIfCancellationRequested();
                Benchmarks.Clear();
                var request = new BenchmarkSearchRequest();
                request.SearchQuery = SearchQuery;
                if (SelectedCategory != "All")
                    request.TradeCategoryId = SelectedCategory;

                var response = await _bridge.SearchBenchmarksAsync(request);
                if (response.IsSuccess)
                {
                    foreach (var b in response.Benchmarks)
                        Benchmarks.Add(b);
                }
                StatusText = "Found " + Benchmarks.Count + " benchmarks";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ApplyBenchmark(object param)
        {
            if (SelectedBenchmark == null) return;
            ProductivityRate = SelectedBenchmark.ProductivityValue;
            CrewSize = SelectedBenchmark.CrewSize;
            StatusText = "Applied benchmark: " + SelectedBenchmark.ActivityDescription;
        }

        [RelayCommand(CanExecute = nameof(CanCalculate))]
        private async Task CalculateAsync()
        {
            if (Quantity <= 0 || ProductivityRate <= 0 || CrewSize < 1 || HoursPerDay <= 0)
                return;

            IsBusy = true;
            try
            {
                var request = new DurationCalculationRequest
                {
                    ActivityId = "ACT-" + (Estimates.Count + 1).ToString("D3"),
                    Quantity = Quantity,
                    ProductivityRate = ProductivityRate,
                    CrewSize = CrewSize,
                    HoursPerDay = HoursPerDay
                };

                var response = await _bridge.CalculateAsync(request);
                if (response.IsSuccess)
                {
                    var item = new DurationEstimateItem
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ActivityId = response.ActivityId,
                        Quantity = Quantity,
                        ProductivityRate = ProductivityRate,
                        CrewSize = CrewSize,
                        HoursPerDay = HoursPerDay,
                        DurationWorkingDays = response.DurationWorkingDays
                    };

                    if (IsVarianceMode)
                    {
                        var optReq = new DurationCalculationRequest
                        {
                            ActivityId = response.ActivityId,
                            Quantity = Quantity,
                            ProductivityRate = OptimisticRate,
                            CrewSize = CrewSize,
                            HoursPerDay = HoursPerDay
                        };
                        var pesReq = new DurationCalculationRequest
                        {
                            ActivityId = response.ActivityId,
                            Quantity = Quantity,
                            ProductivityRate = PessimisticRate,
                            CrewSize = CrewSize,
                            HoursPerDay = HoursPerDay
                        };

                        var optResp = await _bridge.CalculateAsync(optReq);
                        var pesResp = await _bridge.CalculateAsync(pesReq);

                        if (optResp.IsSuccess && pesResp.IsSuccess)
                        {
                            item.OptimisticDuration = optResp.DurationWorkingDays;
                            item.PessimisticDuration = pesResp.DurationWorkingDays;
                            item.ExpectedDuration = (item.OptimisticDuration + 4m * item.DurationWorkingDays + item.PessimisticDuration) / 6m;
                            item.StandardDeviation = (item.PessimisticDuration - item.OptimisticDuration) / 6m;
                        }
                    }

                    Estimates.Add(item);
                    StatusText = "Added " + item.ActivityId + ": " + item.DurationWorkingDays.ToString("F2") + "d";

                    if (IsVarianceMode)
                        StatusText += " (O:" + item.OptimisticDuration.ToString("F2") +
                                      " M:" + item.DurationWorkingDays.ToString("F2") +
                                      " P:" + item.PessimisticDuration.ToString("F2") + ")";

                    OnPropertyChanged(nameof(CanExport));
                }
                else
                {
                    StatusText = "Error: " + response.ErrorMessage;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnRateChanged()
        {
            OnPropertyChanged(nameof(CanCalculate));
            OnPropertyChanged(nameof(CanExport));
            if (Estimates.Count > 0)
            {
                var t = RecalculateAllAsync();
            }
        }

        private async Task RecalculateAllAsync()
        {
            for (int i = 0; i < Estimates.Count; i++)
            {
                var req = new DurationCalculationRequest
                {
                    ActivityId = Estimates[i].ActivityId,
                    Quantity = Estimates[i].Quantity,
                    ProductivityRate = ProductivityRate,
                    CrewSize = CrewSize,
                    HoursPerDay = HoursPerDay
                };

                var resp = await _bridge.CalculateAsync(req);
                if (resp.IsSuccess)
                {
                    var e = Estimates[i];
                    e.DurationWorkingDays = resp.DurationWorkingDays;
                    Estimates[i] = e;
                }
            }
            StatusText = "Recalculated " + Estimates.Count + " estimates";
        }

        [RelayCommand]
        private void ClearEstimates()
        {
            Estimates.Clear();
            StatusText = "Cleared all estimates";
            OnPropertyChanged(nameof(CanExport));
        }

        [RelayCommand(CanExecute = nameof(CanExport))]
        private async Task ExportExcel()
        {
            IsBusy = true;
            try
            {
                StatusText = "Exporting to Excel...";
                var exportReq = new DurationExportRequest();
                foreach (var e in Estimates)
                {
                    exportReq.Estimates.Add(new DurationCalculationResponse
                    {
                        ActivityId = e.ActivityId,
                        DurationWorkingDays = e.DurationWorkingDays,
                        IsSuccess = true
                    });
                }
                exportReq.FilePath = "DurationEstimates.xlsx";

                var result = await _bridge.ExportToExcelAsync(exportReq);
                if (result.IsSuccess)
                    StatusText = "Exported " + result.RowCount + " estimates to " + result.FilePath;
                else
                    StatusText = "Export: " + (result.ErrorMessage ?? "Completed");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleVariance()
        {
            IsVarianceMode = !IsVarianceMode;
        }
    }

    public struct DurationEstimateItem
    {
        public string Id;
        public string ActivityId;
        public decimal Quantity;
        public decimal ProductivityRate;
        public int CrewSize;
        public decimal HoursPerDay;
        public decimal DurationWorkingDays;
        public decimal OptimisticDuration;
        public decimal PessimisticDuration;
        public decimal ExpectedDuration;
        public decimal StandardDeviation;
    }
}
