using Microsoft.Win32;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class FloatPathViewModel : ViewModelBase
    {
        private List<Activity> _activities;
        private Dictionary<string, List<Relationship>> _graph;
        private List<WbsItem> _wbsRaw;
        private readonly FloatPathService _floatPathService;
        private readonly GraphService _graphService;
        private readonly WbsBuilder _wbsBuilder;

        public string SelectedView { get; set; }

        public ObservableCollection<Activity> Activities { get; set; } = new();
        public ObservableCollection<WbsItem> WBSItems { get; } = new();

        [ObservableProperty]
        private Activity? _selectedActivity;

        partial void OnSelectedActivityChanged(Activity? value)
        {
            RunCommand.NotifyCanExecuteChanged();
        }

        public int PathCount { get; set; } = 5;
        public int MaxDepth { get; set; } = 100;
        public bool HighlightCritical { get; set; } = true;
        public int WbsLevel { get; set; }
        public string WbsName { get; set; }

        [ObservableProperty]
        private string _projectName = "No Project Loaded";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _graphHtml;

        public List<string> ViewModes { get; set; } =
            new() { "WBS View", "Network Graph" };

        public bool IsWbsView => SelectedViewMode == "WBS View";
        public bool IsGraphView => SelectedViewMode == "Network Graph";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWbsView), nameof(IsGraphView))]
        private string _selectedViewMode = "WBS View";

        partial void OnSelectedViewModeChanged(string value)
        {
            if (value == "Network Graph")
            {
                ShowWbs = false;
                SendGraphToUI?.Invoke(GraphHtml);
            }
            else
            {
                ShowWbs = true;
            }
        }

        public void SelectActivityById(string id)
        {
            var act = _activities.FirstOrDefault(a => a.Id == id);
            if (act != null)
                SelectedActivity = act;
        }

        [ObservableProperty]
        private bool _showWbs;

        [RelayCommand]
        private void LoadXer()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "XER Files (*.xer)|*.xer"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                IsLoading = true;

                var parser = new XerParser();
                parser.Parse(dialog.FileName);

                _activities = parser.GetActivities();
                _graph = parser.GetRelationships();
                _wbsRaw = parser.GetWBS();

                Activities.Clear();
                foreach (var a in _activities)
                    Activities.Add(a);

                var tree = _wbsBuilder.BuildTree(_wbsRaw, _activities);

                WBSItems.Clear();
                OnPropertyChanged(nameof(WBSItems));
                foreach (var item in tree)
                    WBSItems.Add(item);

                ProjectName = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private void Run()
        {
            if (SelectedViewMode == "WBS View")
                ShowWbs = true;
            else if (SelectedViewMode == "Network Graph")
                ShowWbs = false;

            if (SelectedActivity == null)
                return;

            try
            {
                IsLoading = true;
                var paths = _floatPathService.GetTopPaths(
                    SelectedActivity.Id,
                    _graph,
                    _activities,
                    PathCount,
                    MaxDepth
                );

                if (paths == null || !paths.Any())
                    return;

                var allActivities = paths
                    .SelectMany(p => p.Activities)
                    .Distinct()
                    .ToList();

                var usedIds = allActivities.Select(a => a.Id).ToHashSet();

                var filteredRels = _graph
                    .Where(k => usedIds.Contains(k.Key))
                    .ToDictionary(
                        k => k.Key,
                        k => k.Value.Where(r => usedIds.Contains(r.SuccessorId)).ToList()
                    );

                GraphHtml = _graphService.GenerateGraphHtml(allActivities, filteredRels);
                SendGraphToUI?.Invoke(GraphHtml);

                var excel = new Excel.Application();
                excel.Visible = true;
                excel.DisplayAlerts = false;

                var workbooks = excel.Workbooks;
                Excel.Workbook wb = workbooks.Add();
                var sheets = wb.Sheets;
                Excel.Worksheet ws = (Excel.Worksheet)sheets[1];
                excel.Visible = true;

                try
                {
                    int row = 1;
                    foreach (var path in paths)
                    {
                        ws.Cells[row, 1] = "Path";
                        row++;

                        foreach (var act in path.Activities)
                        {
                            ws.Cells[row, 1] = act.Code;
                            ws.Cells[row, 2] = act.Name;
                            row++;
                        }

                        row++;
                    }
                }
                finally
                {
                    if (ws != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                    if (sheets != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(sheets);
                    if (wb != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
                    if (workbooks != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbooks);
                    if (excel != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanRun => SelectedActivity != null;

        public System.Action<string>? SendGraphToUI;

        public FloatPathViewModel(IServiceContainer container)
        {
            _floatPathService = container.Resolve<FloatPathService>();
            _graphService = container.Resolve<GraphService>();
            _wbsBuilder = container.Resolve<WbsBuilder>();
        }
    }
}
