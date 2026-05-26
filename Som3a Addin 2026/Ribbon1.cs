using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.VisualBasic;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_Addin_2026.Ui;
using Som3a_Addin_2026.UIHost;
using Som3a_WPF_UI;
//using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Services;
//using Som3a_WPF_UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;


namespace Som3a_Addin_2026
{
    public partial class Ribbon1
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            LoadRibbonImages();
            EnsureModuleGroup();
            PopulateAllModuleButtons();
            SubscribeToModuleRibbonEvents();
        }

        private void LoadRibbonImages()
        {
            string imagesFolder =
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources");

            string[] supportedExtensions =
            {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".ico"
    };

            foreach (var tab in this.Tabs)
            {
                foreach (RibbonGroup group in tab.Groups)
                {
                    foreach (var item in group.Items)
                    {
                        if (item is RibbonButton button)
                        {
                            try
                            {
                                string imagePath = supportedExtensions
                                    .Select(ext =>
                                        System.IO.Path.Combine(
                                            imagesFolder,
                                            button.Name + ext))
                                    .FirstOrDefault(System.IO.File.Exists);

                                if (imagePath != null)
                                {
                                    using (var img =
                                        System.Drawing.Image.FromFile(imagePath))
                                    {
                                        button.Image =
                                            ResizeRibbonImage(
                                                new Bitmap(img),
                                                32,
                                                32);

                                        button.ShowImage = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.WriteLine($"Failed to load image for {button.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        private static Bitmap ResizeRibbonImage(
            Bitmap original,
            int width,
            int height)
        {
            var resized = new Bitmap(width, height);

            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(original, 0, 0, width, height);
            }

            return resized;
        }
        private void EnsureModuleGroup()
        {
            try
            {
                _groupModules = this.Factory.CreateRibbonGroup();
                _groupModules.Label = "Modules";
                _groupModules.Name = "groupModules";
                this.tab1.Groups.Add(_groupModules);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to create Modules ribbon group: {ex.Message}");
            }
        }

        private void PopulateAllModuleButtons()
        {
            try
            {
                var orchestrator = Som3a_WPF_UI.App.Container.Resolve<ModuleLoadOrchestrator>();
                var allActions = orchestrator.GetAllModuleRibbonActions();
                foreach (var kvp in allActions)
                {
                    PopulateModuleButtons(kvp.Key, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to populate module ribbon buttons: {ex.Message}");
            }
        }

        private void PopulateModuleButtons(string moduleId, IReadOnlyList<object> actions)
        {
            if (_groupModules is null || actions is null) return;

            var prefix = $"module_{moduleId}_";
            var toRemove = _groupModules.Items
                .OfType<Microsoft.Office.Tools.Ribbon.RibbonControl>()
                .Where(c => c.Name != null && c.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var item in toRemove)
                _groupModules.Items.Remove(item);

            foreach (var action in actions)
            {
                try
                {
                    if (action is RibbonRegistrar.RibbonButtonEntry btnEntry)
                    {
                        var button = this.Factory.CreateRibbonButton();
                        button.Label = btnEntry.Label;
                        button.Name = $"module_{moduleId}_{btnEntry.Id}";
                        button.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
                        button.ShowImage = false;
                        var capturedAction = btnEntry;
                        button.Click += (s, e) => capturedAction.OnClick();
                        _groupModules.Items.Add(button);
                    }
                    else if (action is RibbonRegistrar.RibbonToggleButtonEntry toggleEntry)
                    {
                        var toggle = this.Factory.CreateRibbonToggleButton();
                        toggle.Label = toggleEntry.Label;
                        toggle.Name = $"module_{moduleId}_{toggleEntry.Id}";
                        toggle.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
                        toggle.ShowImage = false;
                        toggle.Checked = toggleEntry.InitialState;
                        var capturedAction = toggleEntry;
                        toggle.Click += (s, e) => capturedAction.OnToggle(toggle.Checked);
                        _groupModules.Items.Add(toggle);
                    }
                    else if (action is RibbonRegistrar.RibbonMenuEntry menuEntry)
                    {
                        var menu = this.Factory.CreateRibbonMenu();
                        menu.Label = menuEntry.Label;
                        menu.Name = $"module_{moduleId}_{menuEntry.Id}";
                        if (menuEntry.Items != null)
                        {
                            foreach (var item in menuEntry.Items)
                            {
                                var subItem = this.Factory.CreateRibbonButton();
                                subItem.Label = item.Label;
                                subItem.Name = $"module_{moduleId}_{menuEntry.Id}_{item.Id}";
                                subItem.ShowImage = false;
                                var capturedItem = item;
                                subItem.Click += (s, e) => capturedItem.OnClick();
                                menu.Items.Add(subItem);
                            }
                        }
                        _groupModules.Items.Add(menu);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Failed to create ribbon action for module '{moduleId}': {ex.Message}");
                }
            }
        }

        private void SubscribeToModuleRibbonEvents()
        {
            try
            {
                var eventBus = Som3a_WPF_UI.App.Container.Resolve<IEventBus>();
                eventBus.Subscribe<ModuleRibbonActionsAvailableEvent>(OnModuleRibbonActionsAvailable);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to subscribe to module ribbon events: {ex.Message}");
            }
        }

        private void OnModuleRibbonActionsAvailable(ModuleRibbonActionsAvailableEvent evt)
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    PopulateModuleButtons(evt.ModuleId, evt.Actions);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed handling module ribbon actions for '{evt.ModuleId}': {ex.Message}");
            }
        }

        private static void LoadImagesFromGroupItems(
            IEnumerable<RibbonGroup> groups,
            Dictionary<string, PropertyInfo> resourceProps)
        {
            foreach (var group in groups)
            {
                foreach (var item in group.Items)
                {
                    if (item is RibbonButton button)
                    {
                        if (!string.IsNullOrEmpty(button.Name) &&
                            resourceProps.TryGetValue(button.Name, out var prop))
                        {
                            button.Image = ResizeRibbonImage((Bitmap)prop.GetValue(null, null), 32, 32);
                            button.ShowImage = true;
                        }
                    }
                    else if (item is RibbonMenu menu)
                    {
                        foreach (RibbonButton subButton in menu.Items.OfType<RibbonButton>())
                        {
                            if (!string.IsNullOrEmpty(subButton.Name) &&
                                resourceProps.TryGetValue(subButton.Name, out var subProp))
                            {
                                subButton.Image = (Bitmap)subProp.GetValue(null, null);
                                subButton.ShowImage = true;
                            }
                        }
                    }
                }
            }
        }

        private void comparsion_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("main");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Comparison Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.piecolors");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Fix Pie Colors Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.tradecodes");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Assign Trade Codes Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        private void btnUnmergeFillDown_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.unmerge");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Unmerge Fill Down Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnDailyReport_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.subdaily");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Sub Daily Report Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        private void btnLinksManager_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.links");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Links Manager Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnProjectAnalysis_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("planning.analysis");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Project Analysis Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }


        private void Coloringwbs_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Excel.Application app = Globals.ThisAddIn.Application;

                var style = WbsStyleFactory.GetStyle(UserSettings.SelectedStyle);
                var svc = new WbsColoringService(style);
                svc.ApplyToActiveSheet(app, "Activity ID", 15);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Coloringwbs");
            }

        }


        private void ExportPaletteHex_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;

                // 1) يكتب HEX جنب الألوان
                new PaletteExportService().ExportSelectedRangeToAdjacentCells(app, false);

                // 2) يكتب اسم المستوى في العمود K بناءً على لون الصف
                var ws = (Excel.Worksheet)app.ActiveSheet;
                var svc = new WbsColoringService();          // <-- لازم تتعرّف هنا
                //svc.WriteWbsLevelNameByRowColor(ws, 11);     // K = 11
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Export Palette HEX");
            }
        }
        private void GroupWbs_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var ws = (Excel.Worksheet)app.ActiveSheet;

                var svc = new WbsColoringService();

                // لو عندك كل مستوى = مسافتين خليها 2
                int spacesPerLevel = 1;

                // preferColorLevels = true => لو الألوان موجودة اعتمد عليها، وإلا ارجع للمسافات
                svc.GroupRowsFull(ws, "Activity ID", 15, spacesPerLevel, true);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Group WBS Full");
            }
        }


        private void btnSafeClone_Click(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;

                if (app.ActiveWorkbook == null)
                {
                    MessageBox.Show("No active workbook.", "Som3a Tools");
                    return;
                }

                using var sfd = new SaveFileDialog();
                sfd.Filter = "Excel Macro-Enabled Workbook (*.xlsm)|*.xlsm|Excel Workbook (*.xlsx)|*.xlsx";
                sfd.FileName = $"{System.IO.Path.GetFileNameWithoutExtension(app.ActiveWorkbook.Name)}_CLEANED.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                // ممكن تعمل نافذة Progress بسيطة أو تستخدم StatusBar
                app.StatusBar = "Safe Clone: starting...";

                var svc = new Som3a.Shared.Core.WorkbookCloneService(app);

                svc.SafeCloneActiveWorkbook(sfd.FileName, p =>
                {
                    app.StatusBar = $"Safe Clone: {p}%";
                });

                app.StatusBar = false;
                MessageBox.Show("Safe Clone created successfully.", "Som3a Tools");
            }
            catch (Exception ex)
            {
                try { Globals.ThisAddIn.Application.StatusBar = false; } catch { }
                System.Windows.MessageBox.Show(ex.Message, "Som3a Tools - Error");
            }
        }

        private void BtnTool_Click(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
        {
            var btn = (Microsoft.Office.Tools.Ribbon.RibbonButton)sender;

            switch (btn.Name)
            {
                case "btnDaysFraction":
                    RunDaysFractionFix();
                    break;

                case "btnPobsCleaner":
                    RunPobsCleaner();
                    break;

                case "btnCalendarsCleanup":
                    RunCalendarsCleanup();
                    break;

                case "btnWbsNormalizer":
                    RunWbsNormalizer();
                    break;

                case "btnIdsRebuild":
                    RunIdsRebuild();
                    break;
            }
        }

        private void RunDaysFractionFix() { /* TODO */ }
        private void RunPobsCleaner() { /* TODO */ }
        private void RunCalendarsCleanup() { /* TODO */ }
        private void RunWbsNormalizer() { /* TODO */ }
        private void RunIdsRebuild() { /* TODO */ }
        private void splitXerTools_Click(object sender, RibbonControlEventArgs e)
        {
            // اختياري: خلي الضغط على جسم الزر يشغل "Run All" أو يفتح Tool رئيسي
            RunDaysFractionFix();
            RunPobsCleaner();
            RunCalendarsCleanup();
            RunWbsNormalizer();
            RunIdsRebuild();
        }

        private void splitButton1_Click(object sender, RibbonControlEventArgs e)
        {

        }
        private void BtnFormatsTool_Click(object sender, RibbonControlEventArgs e)
        {
            var btn = (RibbonButton)sender;

            switch (btn.Name)
            {
                case "btnTool1": /* TODO */ break;
                case "btnTool2": /* TODO */ break;
                case "btnTool3": /* TODO */ break;
                case "btnTool4": /* TODO */ break;
                case "btnTool5": /* TODO */ break;
            }
        }

        private void btnHardReload_Click(object sender, RibbonControlEventArgs e)
        {
            var app = Globals.ThisAddIn.Application;

            try
            {
                CloseAllToolWindows();

                const string addinName = "Som3a Addin 2026"; // بالظبط زي اللي في الديالوج

                Office.COMAddIn target = null;

                foreach (Office.COMAddIn a in app.COMAddIns)
                {
                    if (!string.IsNullOrWhiteSpace(a.Description) &&
                        a.Description.Equals(addinName, StringComparison.OrdinalIgnoreCase))
                    {
                        target = a;
                        break;
                    }
                }

                if (target == null)
                {
                    MessageBox.Show("Som3a Add-in not found in COMAddIns.", "Reload Add-in");
                    return;
                }

                // Disconnect
                target.Connect = false;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(800);

                // Reconnect
                target.Connect = true;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(300);

                MessageBox.Show("Som3a Add-in reloaded successfully.", "Reload Add-in");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Reload Add-in failed");
            }
        }




        private void CloseAllToolWindows()
        {
        }
        public void PrepareForReloadInternal()
        {
            CloseAllToolWindows();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void PrepareForReload()
        {
            try
            {
                Globals.Ribbons.Ribbon1?.PrepareForReloadInternal();
            }
            catch { }
        }

        private void PreForReload_Click(object sender, RibbonControlEventArgs e)
        {
            CloseAllToolWindows();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void button30_Click(object sender, RibbonControlEventArgs e)
        {

        }
        private RibbonGroup _groupModules;

        private void Float_Path_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("analysis.floatpath");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Float Path Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void color_setting_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("excel.styles");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Style Selector Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void Xer_Editor_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("planning.xereditor");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "XER Editor Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void Com_Xer_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("planning.primavera.compare");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Primavera Compare Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnWorkspace_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("welcome");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Workspace Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void addin_setting_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                NavigationService.Instance.NavigateTo("settings.general");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "Settings Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }


    }
    }


