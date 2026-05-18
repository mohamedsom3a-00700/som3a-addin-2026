using Microsoft.Win32;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Som3a_WPF_UI.ViewModels
{
    public class FloatPathViewModel : INotifyPropertyChanged
    {
        #region Fields

        private List<Activity> _activities;
        private Dictionary<string, List<Relationship>> _graph;
        private List<WbsItem> _wbsRaw;
        public string SelectedView { get; set; }
        #endregion

        #region Bindings

        public ObservableCollection<Activity> Activities { get; set; } = new();
        public ObservableCollection<WbsItem> WBSItems { get; } = new();

        private Activity _selectedActivity;
        public Activity SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                _selectedActivity = value;
                OnPropertyChanged();

                // 🔥 دي أهم سطر
                (RunCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int PathCount { get; set; } = 5;
        public int MaxDepth { get; set; } = 100;
        public bool HighlightCritical { get; set; } = true;
        public int WbsLevel { get; set; }
        public string WbsName { get; set; }

        public string ProjectName { get; set; } = "No Project Loaded";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _graphHtml;
        public string GraphHtml
        {
            get => _graphHtml;
            set { _graphHtml = value; OnPropertyChanged(); }
        }

        #endregion

        #region View Mode
        private void BuildGraphOnly()
        {
            var service = new FloatPathService();

            var paths = service.GetTopPaths(
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

            var graphService = new GraphService();
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cytoscape.min.js");

            GraphHtml = graphService.GenerateGraphHtml(allActivities, filteredRels);
            MessageBox.Show($"ShowGraph = {IsGraphView}");
            SendGraphToUI?.Invoke(GraphHtml);
        }
        public List<string> ViewModes { get; set; } =
            new() { "WBS View", "Network Graph" };
        public bool IsWbsView => SelectedViewMode == "WBS View";
        public bool IsGraphView => SelectedViewMode == "Network Graph";

        private string _selectedViewMode = "WBS View";
        public string SelectedViewMode
        {
            get => _selectedViewMode;
            set
            {
                _selectedViewMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWbsView));
                OnPropertyChanged(nameof(IsGraphView));

                // 🔥 الحل هنا
                if (_selectedViewMode == "Network Graph")
                {
                    ShowWbs = false;

                    MessageBox.Show("Get Graph View");

                    // 🔥 شيل الشرط خالص
                    SendGraphToUI?.Invoke(GraphHtml);
                }
                // 🔥 تأخير بسيط عشان WebView2 يبقى visible
                // Task.Run(async () =>
                // {
                // await Task.Delay(100);

                //  Application.Current.Dispatcher.Invoke(() =>
                // {
                // BuildGraphOnly();
                // });
                //  });
                else
                {
                    ShowWbs = true;

                    MessageBox.Show("Get Wbs View");
                }
            }
        }
        public void SelectActivityById(string id)
        {
            var act = _activities.FirstOrDefault(a => a.Id == id);

            if (act != null)
            {
                SelectedActivity = act;
            }
        }


        #endregion

        #region Commands

        public ICommand LoadXerCommand { get; }
        public ICommand RunCommand { get; }

        #endregion

        public FloatPathViewModel()
        {
            LoadXerCommand = new RelayCommand(LoadXer);
            RunCommand = new RelayCommand(Run, () => SelectedActivity != null);
        }

        #region Load

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

                // Activities
                Activities.Clear();
                foreach (var a in _activities)
                    Activities.Add(a);

                // WBS Tree
                var builder = new WbsBuilder();
                var tree = builder.BuildTree(_wbsRaw, _activities);

                WBSItems.Clear();
                OnPropertyChanged(nameof(WBSItems));
                // WBS Tree

                WBSItems.Clear();
                foreach (var item in tree)
                    WBSItems.Add(item);



                ProjectName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                OnPropertyChanged(nameof(ProjectName));
                MessageBox.Show($"Activities: {_activities.Count}");
                MessageBox.Show($"WBS: {_wbsRaw.Count}");
                MessageBox.Show($"Tree Roots: {WBSItems.Count}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Run
        private bool _showWbs;
        public bool ShowWbs
        {
            get => _showWbs;
            set
            {
                _showWbs = value;
                OnPropertyChanged(nameof(ShowWbs));
            }
        }

        //private bool _showGraph;
        // public bool ShowGraph
        // {
        // get => _showGraph;
        // set
        //     {
        // _showGraph = value;
        // OnPropertyChanged(nameof(ShowGraph));
        // }
        //}



        private void Run()
        {
            MessageBox.Show("Run Clicked ✅");
            if (SelectedViewMode == "WBS View")
            {
                ShowWbs = true;
            }
            else if (SelectedViewMode == "Network Graph")
            {
                ShowWbs = false;
            }

            if (SelectedActivity == null)
            {
                MessageBox.Show("Select Activity first");
                return;
            }

            try
            {
                IsLoading = true;
                MessageBox.Show($"Start ID = {SelectedActivity?.Id}");
                var service = new FloatPathService();

                var paths = service.GetTopPaths(
                    SelectedActivity.Id,
                    _graph,
                    _activities,
                    PathCount,
                    MaxDepth
                );

                MessageBox.Show($"Paths Count: {paths?.Count}");

                if (paths == null || !paths.Any())
                {
                    MessageBox.Show("No Path Found ❌");
                    return;
                }

                var allActivities = paths
                    .SelectMany(p => p.Activities)
                    .Distinct()
                    .ToList();

                MessageBox.Show($"Activities in Graph: {allActivities.Count}");

                var graphService = new GraphService();
                var usedIds = allActivities.Select(a => a.Id).ToHashSet();

                var filteredRels = _graph
                    .Where(k => usedIds.Contains(k.Key))
                    .ToDictionary(
                        k => k.Key,
                        k => k.Value.Where(r => usedIds.Contains(r.SuccessorId)).ToList()
                    );
                var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cytoscape.min.js");

                GraphHtml = graphService.GenerateGraphHtml(allActivities, filteredRels);
                MessageBox.Show(File.Exists(jsPath)
    ? "cytoscape FOUND ✅"
    : "cytoscape NOT FOUND ❌");
                MessageBox.Show($"Nodes: {_activities.Count}");
                MessageBox.Show($"Edges: {_graph.Sum(x => x.Value.Count)}");

                MessageBox.Show("Graph Generated ✅");

                // 🔥 اختبار الجراف خارج البرنامج
               // File.WriteAllText("test.html", GraphHtml);
                //System.Diagnostics.Process.Start("test.html");

                //❌ اقفل ده مؤقتًا
                SendGraphToUI?.Invoke(GraphHtml);

                var excel = new Excel.Application();
                excel.Visible = true;
                excel.DisplayAlerts = false;

                Excel.Workbook wb = excel.Workbooks.Add();
                Excel.Worksheet ws = (Excel.Worksheet)wb.Sheets[1];
                excel.Visible = true;

                int row = 1;
                MessageBox.Show("Before Excel");
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
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Event to UI

        public System.Action<string> SendGraphToUI;

        #endregion

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}