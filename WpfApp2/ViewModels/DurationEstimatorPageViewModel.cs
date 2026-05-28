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
    public class DurationEstimatorPageViewModel : ViewModelBase
    {
        private readonly IDurationEstimatorBridge _bridge;
        private bool _isBusy;
        private string _statusText = "Ready";
        private decimal _productivityRate = 10m;
        private int _crewSize = 2;
        private decimal _hoursPerDay = 8m;
        private decimal _quantity;
        private bool _isVarianceMode;
        private decimal _optimisticRate = 12m;
        private decimal _pessimisticRate = 7m;
        private string _searchQuery = "";
        private string _selectedCategory = "All";
        private BenchmarkDto _selectedBenchmark;

        public DurationEstimatorPageViewModel(IDurationEstimatorBridge bridge)
        {
            _bridge = bridge;
            CalculateCommand = new RelayCommand(async o => await CalculateAsync(), o => CanCalculate);
            SearchBenchmarksCommand = new RelayCommand(async o => await SearchBenchmarksAsync());
            ApplyBenchmarkCommand = new RelayCommand(ApplyBenchmark);
            ExportExcelCommand = new RelayCommand(async o => await ExportToExcelAsync(), o => CanExport);
            ClearCommand = new RelayCommand(o => ClearEstimates());
            ToggleVarianceCommand = new RelayCommand(o => { IsVarianceMode = !IsVarianceMode; });

            Categories = new List<string>
            {
                "All", "concrete", "steel", "masonry", "mep",
                "finishes", "earthwork", "roofing", "glazing", "flooring"
            };
            _selectedCategory = "All";
            var t = LoadBenchmarksAsync();
        }

        public ObservableCollection<DurationEstimateItem> Estimates { get; } = new ObservableCollection<DurationEstimateItem>();
        public ObservableCollection<BenchmarkDto> Benchmarks { get; } = new ObservableCollection<BenchmarkDto>();
        public int BenchmarkCount { get { return Benchmarks.Count; } }
        public int EstimateCount { get { return Estimates.Count; } }
        public List<string> Categories { get; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value ?? ""); }
        }

        public decimal ProductivityRate
        {
            get { return _productivityRate; }
            set { if (SetProperty(ref _productivityRate, value)) OnRateChanged(); }
        }

        public int CrewSize
        {
            get { return _crewSize; }
            set { if (SetProperty(ref _crewSize, value)) OnRateChanged(); }
        }

        public decimal HoursPerDay
        {
            get { return _hoursPerDay; }
            set { if (SetProperty(ref _hoursPerDay, value)) OnRateChanged(); }
        }

        public decimal Quantity
        {
            get { return _quantity; }
            set { if (SetProperty(ref _quantity, value)) OnPropertyChanged(nameof(CanCalculate)); }
        }

        public bool IsVarianceMode
        {
            get { return _isVarianceMode; }
            set { if (SetProperty(ref _isVarianceMode, value)) OnPropertyChanged(nameof(CanCalculate)); }
        }

        public decimal OptimisticRate
        {
            get { return _optimisticRate; }
            set { if (SetProperty(ref _optimisticRate, value)) OnRateChanged(); }
        }

        public decimal PessimisticRate
        {
            get { return _pessimisticRate; }
            set { if (SetProperty(ref _pessimisticRate, value)) OnRateChanged(); }
        }

        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetProperty(ref _searchQuery, value ?? ""); }
        }

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    var t = SearchBenchmarksAsync();
                }
            }
        }

        public BenchmarkDto SelectedBenchmark
        {
            get { return _selectedBenchmark; }
            set { SetProperty(ref _selectedBenchmark, value); }
        }

        public bool CanCalculate
        {
            get { return !IsBusy && Quantity > 0 && ProductivityRate > 0 && CrewSize >= 1 && HoursPerDay > 0; }
        }

        public bool CanExport
        {
            get { return !IsBusy && Estimates.Count > 0; }
        }

        public ICommand CalculateCommand { get; }
        public ICommand SearchBenchmarksCommand { get; }
        public ICommand ApplyBenchmarkCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ToggleVarianceCommand { get; }

        public event Action RequestClose;

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

        private async Task SearchBenchmarksAsync()
        {
            IsBusy = true;
            try
            {
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

        private void ApplyBenchmark(object param)
        {
            if (SelectedBenchmark == null) return;
            ProductivityRate = SelectedBenchmark.ProductivityValue;
            CrewSize = SelectedBenchmark.CrewSize;
            StatusText = "Applied benchmark: " + SelectedBenchmark.ActivityDescription;
        }

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

                        item.OptimisticDuration = optResp.DurationWorkingDays;
                        item.PessimisticDuration = pesResp.DurationWorkingDays;
                        item.ExpectedDuration = (item.OptimisticDuration + 4m * item.DurationWorkingDays + item.PessimisticDuration) / 6m;
                        item.StandardDeviation = (item.PessimisticDuration - item.OptimisticDuration) / 6m;
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

        private void ClearEstimates()
        {
            Estimates.Clear();
            StatusText = "Cleared all estimates";
            OnPropertyChanged(nameof(CanExport));
        }

        private async Task ExportToExcelAsync()
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
