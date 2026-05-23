using Microsoft.Office.Interop.Excel;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Threading;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using SysAction = System.Action;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class LinksManagerViewModel : INotifyPropertyChanged
    {
        private readonly Dispatcher _ui;
        private readonly ExcelApp _app;
        private readonly LinksManagerService _service;

        public ObservableCollection<WorkbookItem> Workbooks { get; } = new();
        public ObservableCollection<LinkTypeItem> LinkTypes { get; } = new();
        public ObservableCollection<LinkItem> Links { get; } = new();

        private readonly ICollectionView _linksView;

        private WorkbookItem? _selectedWorkbook;
        public WorkbookItem? SelectedWorkbook
        {
            get => _selectedWorkbook;
            set
            {
                if (Set(ref _selectedWorkbook, value))
                {
                    StatusText = value == null ? "Select a workbook." : $"Workbook: {value.Name}";
                    RaiseAllCanExecute();
                }
            }
        }

        private LinkTypeItem? _selectedType;
        public LinkTypeItem? SelectedType
        {
            get => _selectedType;
            set
            {
                if (Set(ref _selectedType, value))
                {
                    StatusText = value == null ? "Select a link type." : $"Type: {value.Key}";
                    RaiseAllCanExecute();
                }
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Set(ref _searchText, value ?? ""))
                    _linksView.Refresh();
            }
        }

        private int _progressPercent;
        public int ProgressPercent
        {
            get => _progressPercent;
            set => Set(ref _progressPercent, Math.Max(0, Math.Min(100, value)));
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (Set(ref _isBusy, value))
                {
                    RaiseAllCanExecute();
                    OnPropertyChanged(nameof(CanSelectAll));
                    OnPropertyChanged(nameof(CanUnselectAll));
                    OnPropertyChanged(nameof(CanBreak));
                }
            }
        }

        private string _statusText = "Ready.";
        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value ?? "");
        }

        public bool HasSelection => Links.Any(x => x.IsSelected);
        public string SelectedCountText => $"{Links.Count(x => x.IsSelected)} selected";

        public bool CanSelectAll => !IsBusy && Links.Count > 0 && Links.Any(x => !x.IsSelected);
        public bool CanUnselectAll => !IsBusy && Links.Count > 0 && Links.Any(x => x.IsSelected);

        public bool CanBreak => !IsBusy && SelectedWorkbook != null &&
                               (HasSelection || SelectedType?.Key == "LinkSources");

        public event SysAction? RequestClose;

        public RelayCommand RefreshWorkbooksCommand { get; }
        public RelayCommand LoadTypesCommand { get; }
        public RelayCommand ReloadLinksCommand { get; }
        public RelayCommand BreakSelectedCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand SelectAllLinksCommand { get; }
        public RelayCommand UnselectAllLinksCommand { get; }

        public LinksManagerViewModel(IServiceContainer container, ExcelApp app, Dispatcher uiDispatcher, LinksManagerService service)
        {
            _ui = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            _linksView = CollectionViewSource.GetDefaultView(Links);
            _linksView.Filter = FilterLinks;

            RefreshWorkbooksCommand = new RelayCommand(RefreshWorkbooks, () => !IsBusy);
            LoadTypesCommand = new RelayCommand(LoadTypesBasedOnWorkbook, () => !IsBusy && SelectedWorkbook != null);
            ReloadLinksCommand = new RelayCommand(ReloadLinks, () => !IsBusy && SelectedWorkbook != null && SelectedType != null);

            BreakSelectedCommand = new RelayCommand(
                () => _ = BreakSelectedSmartAsync(),
                () => CanBreak
            );

            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());

            SelectAllLinksCommand = new RelayCommand(SelectAll, () => CanSelectAll);
            UnselectAllLinksCommand = new RelayCommand(UnselectAll, () => CanUnselectAll);

            HookLinksSelectionEvents();

            RefreshWorkbooks();

            LinkTypes.Clear();
            foreach (var t in _service.LoadAllLinkTypes())
                LinkTypes.Add(t);
        }

        private bool FilterLinks(object obj)
        {
            if (obj is not LinkItem li) return false;
            var s = (SearchText ?? "").Trim().ToLowerInvariant();
            if (s.Length == 0) return true;

            return (li.LinkSource ?? "").ToLowerInvariant().Contains(s)
                || (li.SheetOrObject ?? "").ToLowerInvariant().Contains(s)
                || (li.CellOrDetail ?? "").ToLowerInvariant().Contains(s)
                || (li.Type ?? "").ToLowerInvariant().Contains(s);
        }

        private void RefreshWorkbooks()
        {
            try
            {
                IsBusy = true;
                Workbooks.Clear();

                foreach (var wb in _service.GetOpenWorkbooks())
                    Workbooks.Add(wb);

                if (SelectedWorkbook == null && Workbooks.Count > 0)
                    SelectedWorkbook = Workbooks[0];

                StatusText = "Workbooks refreshed.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadTypesBasedOnWorkbook()
        {
            if (SelectedWorkbook == null) return;

            var wb = _service.TryGetWorkbookByName(SelectedWorkbook.Name);
            if (wb == null)
            {
                StatusText = "Workbook not found.";
                return;
            }

            try
            {
                IsBusy = true;
                ProgressPercent = 0;
                StatusText = "Scanning available types...";

                var types = _service.LoadTypesBasedOnWorkbook(wb, p => ProgressPercent = p);

                LinkTypes.Clear();
                foreach (var t in types) LinkTypes.Add(t);

                SelectedType = LinkTypes.FirstOrDefault();
                StatusText = "Types loaded based on the selected workbook.";
            }
            finally
            {
                ProgressPercent = 0;
                IsBusy = false;
            }
        }

        private void ReloadLinks()
        {
            if (SelectedWorkbook == null || SelectedType == null) return;

            var wb = _service.TryGetWorkbookByName(SelectedWorkbook.Name);
            if (wb == null)
            {
                StatusText = "Workbook not found.";
                return;
            }

            try
            {
                IsBusy = true;
                ProgressPercent = 0;

                Links.Clear();

                var rows = _service.LoadLinksByType(wb, SelectedType.Key, p => ProgressPercent = p);
                foreach (var r in rows) Links.Add(r);

                RefreshSelectionUi();
                StatusText = $"Loaded {Links.Count} link(s).";
            }
            finally
            {
                ProgressPercent = 0;
                IsBusy = false;
            }
        }

        private async System.Threading.Tasks.Task BreakSelectedSmartAsync()
        {
            if (SelectedWorkbook == null) return;

            var wb = _service.TryGetWorkbookByName(SelectedWorkbook.Name);
            if (wb == null) { StatusText = "Workbook not found."; return; }

            var selected = Links.Where(x => x.IsSelected).ToList();

            try
            {
                IsBusy = true;
                ProgressPercent = 0;

                if (SelectedType?.Key == "LinkSources")
                {
                    StatusText = "Breaking ALL workbook links (LinkSources)...";

                    foreach (var it in Links)
                    {
                        it.Status = "Queued";
                        it.IsBreaking = true;
                    }

                    int broken = await _service.BreakAllLinkSourcesAsync(
                        wb,
                        onProgress: p => _ui.BeginInvoke(new System.Action(() => ProgressPercent = p))
                    );

                    _ui.BeginInvoke(new System.Action(() =>
                    {
                        foreach (var it in Links)
                        {
                            it.IsBreaking = false;
                            it.Status = "Broken";
                            it.IsSelected = false;
                        }

                        RefreshSelectionUi();
                        StatusText = $"Done. Broken workbook links: {broken}.";
                    }));

                    return;
                }

                if (selected.Count == 0)
                {
                    StatusText = "Select one or more links.";
                    return;
                }

                StatusText = "Breaking (smart auto-detect)...";

                foreach (var it in selected)
                {
                    it.Status = "Queued";
                    it.IsBreaking = false;
                }

                int changed = await _service.BreakLinksSmartAsync(
                    wb,
                    selected,
                    onItemUpdate: (item, status, isBreaking, err) =>
                        _ui.BeginInvoke(new System.Action(() =>
                        {
                            item.Status = status;
                            item.IsBreaking = isBreaking;
                            if (!string.IsNullOrWhiteSpace(err))
                                StatusText = err;
                        })),
                    onProgress: p =>
                        _ui.BeginInvoke(new System.Action(() => ProgressPercent = p))
                );

                _ui.BeginInvoke(new System.Action(() =>
                {
                    foreach (var it in selected)
                        it.IsSelected = false;

                    RefreshSelectionUi();
                    StatusText = $"Done. Total changed: {changed}.";
                }));
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
            }
            finally
            {
                _ui.BeginInvoke(new System.Action(() =>
                {
                    ProgressPercent = 0;
                    IsBusy = false;
                }));
            }
        }

        private void SelectAll()
        {
            foreach (var x in Links) x.IsSelected = true;
            RefreshSelectionUi();
        }

        private void UnselectAll()
        {
            foreach (var x in Links) x.IsSelected = false;
            RefreshSelectionUi();
        }

        private void HookLinksSelectionEvents()
        {
            Links.CollectionChanged += Links_CollectionChanged;
        }

        private void Links_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (LinkItem it in e.OldItems)
                    it.PropertyChanged -= Link_PropertyChanged;

            if (e.NewItems != null)
                foreach (LinkItem it in e.NewItems)
                    it.PropertyChanged += Link_PropertyChanged;

            RefreshSelectionUi();
        }

        private void Link_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LinkItem.IsSelected))
                RefreshSelectionUi();
        }

        private void RefreshSelectionUi()
        {
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectedCountText));
            OnPropertyChanged(nameof(CanSelectAll));
            OnPropertyChanged(nameof(CanUnselectAll));
            OnPropertyChanged(nameof(CanBreak));

            BreakSelectedCommand.RaiseCanExecuteChanged();
            SelectAllLinksCommand.RaiseCanExecuteChanged();
            UnselectAllLinksCommand.RaiseCanExecuteChanged();
        }

        private void RaiseAllCanExecute()
        {
            RefreshWorkbooksCommand.RaiseCanExecuteChanged();
            LoadTypesCommand.RaiseCanExecuteChanged();
            ReloadLinksCommand.RaiseCanExecuteChanged();
            BreakSelectedCommand.RaiseCanExecuteChanged();
            CloseCommand.RaiseCanExecuteChanged();
            SelectAllLinksCommand.RaiseCanExecuteChanged();
            UnselectAllLinksCommand.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
