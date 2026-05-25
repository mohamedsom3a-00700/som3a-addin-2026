using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private readonly SettingsPersistenceService _settingsService;

        private UserSettings _currentSettings;
        private UserSettings _previewSettings;
        private SettingsCategory? _selectedCategory;
        private bool _isDirty;
        private bool _isPreviewActive;
        private string _originalTheme;
        private string _originalAccent;
        private object? _currentPanel;

        public ObservableCollection<SettingsCategory> Categories { get; } = new();
        public ObservableCollection<AccentSwatchItem> AccentSwatches { get; } = new();

        public SettingsCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    if (IsPreviewActive)
                    {
                        CancelPreviewCommand.Execute(null);
                    }
                    OnPropertyChanged(nameof(SelectedCategory));
                    UpdateCurrentPanel();
                }
            }
        }

        private void UpdateCurrentPanel()
        {
            if (_selectedCategory?.PanelType == null)
            {
                CurrentPanel = null;
                return;
            }
            CurrentPanel = Activator.CreateInstance(_selectedCategory.PanelType);
        }

        public object? CurrentPanel
        {
            get => _currentPanel;
            set => SetProperty(ref _currentPanel, value);
        }

        public UserSettings CurrentSettings
        {
            get => _currentSettings;
            set => SetProperty(ref _currentSettings, value);
        }

        public UserSettings PreviewSettings
        {
            get => _previewSettings;
            set => SetProperty(ref _previewSettings, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public bool IsPreviewActive
        {
            get => _isPreviewActive;
            set => SetProperty(ref _isPreviewActive, value);
        }

        // Theme selection helpers
        public bool IsDarkSelected => _previewSettings.SelectedTheme == "Dark";
        public bool IsLightSelected => _previewSettings.SelectedTheme == "Light";
        public bool IsCustomSelected => _previewSettings.SelectedTheme == "Custom";
        public bool AccentSwatchesVisible => _previewSettings.SelectedTheme == "Custom";

        // Animation & density helpers
        public bool AnimationOff => _previewSettings.AnimationSpeed == "Off";
        public bool AnimationReduced => _previewSettings.AnimationSpeed == "Reduced";
        public bool AnimationFull => _previewSettings.AnimationSpeed == "Full";
        public bool DensityCompact => _previewSettings.UiDensity == "Compact";
        public bool DensityNormal => _previewSettings.UiDensity == "Normal";
        public bool DensitySpacious => _previewSettings.UiDensity == "Spacious";
        public bool BackgroundSolid => _previewSettings.BackgroundStyle == "Solid";
        public bool BackgroundGradient => _previewSettings.BackgroundStyle == "Gradient";

        // Commands
        public ICommand ThemeCardCommand { get; }
        public ICommand AccentSwatchCommand { get; }
        public ICommand ApplyThemeCommand { get; }
        public ICommand CancelPreviewCommand { get; }
        public ICommand ExportSettingsCommand { get; }
        public ICommand ImportSettingsCommand { get; }
        public ICommand AnimationSpeedCommand { get; }
        public ICommand DensityCommand { get; }
        public ICommand BackgroundStyleCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool?>? CloseWindow { get; set; }

        public DiagnosticsViewModel DiagnosticsVM { get; }

        public SettingsViewModel(SettingsPersistenceService settingsService, DiagnosticsViewModel diagnosticsVm)
        {
            _themeManager = ThemeManager.Instance;
            _settingsService = settingsService;
            DiagnosticsVM = diagnosticsVm;

            _currentSettings = _settingsService.LoadSettings();
            _previewSettings = CloneSettings(_currentSettings);

            _originalTheme = _currentSettings.SelectedTheme;
            _originalAccent = _currentSettings.AccentColor;

            InitializeCategories();
            InitializeSwatches();

            ThemeCardCommand = new RelayCommand(param => OnThemeCardClick(param as string));
            AccentSwatchCommand = new RelayCommand(param => OnAccentSwatchClick(param as string));
            ApplyThemeCommand = new RelayCommand(OnApplyTheme);
            CancelPreviewCommand = new RelayCommand(OnCancelPreview);
            ExportSettingsCommand = new RelayCommand(OnExportSettings);
            ImportSettingsCommand = new RelayCommand(OnImportSettings);
            AnimationSpeedCommand = new RelayCommand(param => OnAnimationSpeedChanged(param as string));
            DensityCommand = new RelayCommand(param => OnDensityChanged(param as string));
            BackgroundStyleCommand = new RelayCommand(param => OnBackgroundStyleChanged(param as string));
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);

            _themeManager.ThemeChanged += OnThemeChanged;
        }

        private void InitializeCategories()
        {
            Categories.Add(new SettingsCategory
            {
                Id = "appearance",
                DisplayName = "Appearance",
                Icon = "\u26A1",
                PanelType = typeof(Views.AppearancePanel),
                Order = 1
            });
            Categories.Add(new SettingsCategory
            {
                Id = "performance",
                DisplayName = "Performance",
                Icon = "\u2699",
                PanelType = typeof(Views.PerformancePanel),
                Order = 2
            });
            Categories.Add(new SettingsCategory
            {
                Id = "accessibility",
                DisplayName = "Accessibility",
                Icon = "\u267F",
                PanelType = typeof(Views.AccessibilityPanel),
                Order = 3
            });
            Categories.Add(new SettingsCategory
            {
                Id = "diagnostics",
                DisplayName = "Diagnostics",
                Icon = "\u2139",
                PanelType = typeof(Views.DiagnosticsPanel),
                Order = 4
            });
            Categories.Add(new SettingsCategory
            {
                Id = "excel",
                DisplayName = "Excel",
                Icon = "\uD83D\uDCC4",
                PanelType = typeof(Views.ExcelPanel),
                Order = 5
            });
            Categories.Add(new SettingsCategory
            {
                Id = "plugins",
                DisplayName = "Plugins",
                Icon = "\uD83D\uDD17",
                PanelType = typeof(Views.PluginsPanel),
                Order = 6
            });

            if (Categories.Count > 0)
            {
                _selectedCategory = Categories[0];
                UpdateCurrentPanel();
            }
        }

        private void InitializeSwatches()
        {
            AccentSwatches.Add(new AccentSwatchItem("#3A86FF", "Blue", "Blue accent"));
            AccentSwatches.Add(new AccentSwatchItem("#2ED573", "Green", "Green accent"));
            AccentSwatches.Add(new AccentSwatchItem("#A855F7", "Purple", "Purple accent"));
            AccentSwatches.Add(new AccentSwatchItem("#FFA502", "Orange", "Orange accent"));
            AccentSwatches.Add(new AccentSwatchItem("#EC4899", "Pink", "Pink accent"));
            AccentSwatches.Add(new AccentSwatchItem("#14B8A6", "Teal", "Teal accent"));
            AccentSwatches.Add(new AccentSwatchItem("#EF4444", "Red", "Red accent"));
            AccentSwatches.Add(new AccentSwatchItem("#06B6D4", "Cyan", "Cyan accent"));

            UpdateSwatchSelection();
        }

        private void OnThemeCardClick(string? themeName)
        {
            if (string.IsNullOrEmpty(themeName)) return;
            if (!IsPreviewActive)
            {
                _previewSettings = CloneSettings(_currentSettings);
            }
            _previewSettings.SelectedTheme = themeName;
            var accent = themeName == "Custom" ? _previewSettings.AccentColor : null;
            _themeManager.ApplyTheme(themeName, accent);
            IsPreviewActive = true;
            RefreshPreviewBindings();
        }

        private void OnAccentSwatchClick(string? hex)
        {
            if (string.IsNullOrEmpty(hex)) return;
            if (!IsPreviewActive)
            {
                _previewSettings = CloneSettings(_currentSettings);
            }
            _previewSettings.AccentColor = hex;
            _previewSettings.SelectedTheme = "Custom";
            _themeManager.ApplyTheme("Custom", hex);
            IsPreviewActive = true;
            UpdateSwatchSelection();
            RefreshPreviewBindings();
        }

        private void OnAnimationSpeedChanged(string? speed)
        {
            if (string.IsNullOrEmpty(speed)) return;
            _previewSettings.AnimationSpeed = speed;
            IsDirty = true;
            RefreshPreviewBindings();
        }

        private void OnDensityChanged(string? density)
        {
            if (string.IsNullOrEmpty(density)) return;
            _previewSettings.UiDensity = density;
            IsDirty = true;
            RefreshPreviewBindings();
        }

        private void OnBackgroundStyleChanged(string? style)
        {
            if (string.IsNullOrEmpty(style)) return;
            _previewSettings.BackgroundStyle = style;
            IsDirty = true;
            RefreshPreviewBindings();
        }

        private void OnApplyTheme()
        {
            _currentSettings = CloneSettings(_previewSettings);
            _settingsService.SaveSettings(_currentSettings);
            _originalTheme = _currentSettings.SelectedTheme;
            _originalAccent = _currentSettings.AccentColor;
            IsPreviewActive = false;
            IsDirty = false;
        }

        private void OnCancelPreview()
        {
            _previewSettings = CloneSettings(_currentSettings);
            _themeManager.ApplyTheme(_originalTheme, _originalAccent);
            IsPreviewActive = false;
            RefreshPreviewBindings();
            UpdateSwatchSelection();
        }

        private void OnExportSettings()
        {
            try
            {
                var filePath = GetExportFilePath();
                if (string.IsNullOrEmpty(filePath)) return;
                _settingsService.ExportSettings(_currentSettings, filePath);
                ToastService.Success("Settings exported successfully.");
            }
            catch (Exception ex)
            {
                ToastService.Error($"Export failed: {ex.Message}");
            }
        }

        private void OnImportSettings()
        {
            var filePath = GetImportFilePath();
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                var result = _settingsService.ImportSettings(filePath);
                _currentSettings = result.Settings;
                _previewSettings = CloneSettings(_currentSettings);
                _themeManager.ApplyTheme(_currentSettings.SelectedTheme,
                    _currentSettings.SelectedTheme == "Custom" ? _currentSettings.AccentColor : null);
                _settingsService.SaveSettings(_currentSettings);
                _originalTheme = _currentSettings.SelectedTheme;
                _originalAccent = _currentSettings.AccentColor;
                RefreshPreviewBindings();
                UpdateSwatchSelection();

                if (result.Warnings.Count > 0)
                {
                    var warnings = string.Join("\n", result.Warnings);
                    ToastService.Warning($"Settings imported with warnings:\n{warnings}");
                }
                else
                {
                    ToastService.Success("Settings imported successfully.");
                }
            }
            catch (FileNotFoundException)
            {
                ToastService.Error("Settings file not found.");
            }
            catch (SettingsImportException ex)
            {
                ToastService.Error($"Import failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                ToastService.Error($"Unexpected error: {ex.Message}");
            }
        }

        private static string? GetExportFilePath()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "Som3a-Settings.json"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private static string? GetImportFilePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private void OnExportSettings()
        {
            try
            {
                var filePath = GetExportFilePath();
                if (string.IsNullOrEmpty(filePath)) return;
                _settingsService.ExportSettings(_currentSettings, filePath);
                ToastService.Success("Settings exported successfully.");
            }
            catch (Exception ex)
            {
                ToastService.Error($"Export failed: {ex.Message}");
            }
        }

        private void OnImportSettings()
        {
            var filePath = GetImportFilePath();
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                var result = _settingsService.ImportSettings(filePath);
                _currentSettings = result.Settings;
                _previewSettings = CloneSettings(_currentSettings);
                _themeManager.ApplyTheme(_currentSettings.SelectedTheme,
                    _currentSettings.SelectedTheme == "Custom" ? _currentSettings.AccentColor : null);
                _settingsService.SaveSettings(_currentSettings);
                _originalTheme = _currentSettings.SelectedTheme;
                _originalAccent = _currentSettings.AccentColor;
                RefreshPreviewBindings();
                UpdateSwatchSelection();

                if (result.Warnings.Count > 0)
                {
                    var warnings = string.Join("\n", result.Warnings);
                    ToastService.Warning($"Settings imported with warnings:\n{warnings}");
                }
                else
                {
                    ToastService.Success("Settings imported successfully.");
                }
            }
            catch (FileNotFoundException)
            {
                ToastService.Error("Settings file not found.");
            }
            catch (SettingsImportException ex)
            {
                ToastService.Error($"Import failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                ToastService.Error($"Unexpected error: {ex.Message}");
            }
        }

        private static string? GetExportFilePath()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "Som3a-Settings.json"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private static string? GetImportFilePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".json"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private void UpdateSwatchSelection()
        {
            foreach (var swatch in AccentSwatches)
                swatch.IsSelected = swatch.Hex.Equals(_previewSettings.AccentColor, StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshPreviewBindings()
        {
            OnPropertyChanged(nameof(IsDarkSelected));
            OnPropertyChanged(nameof(IsLightSelected));
            OnPropertyChanged(nameof(IsCustomSelected));
            OnPropertyChanged(nameof(AccentSwatchesVisible));
            OnPropertyChanged(nameof(AnimationOff));
            OnPropertyChanged(nameof(AnimationReduced));
            OnPropertyChanged(nameof(AnimationFull));
            OnPropertyChanged(nameof(DensityCompact));
            OnPropertyChanged(nameof(DensityNormal));
            OnPropertyChanged(nameof(DensitySpacious));
            OnPropertyChanged(nameof(BackgroundSolid));
            OnPropertyChanged(nameof(BackgroundGradient));
            OnPropertyChanged(nameof(PreviewSettings));
        }

        private void OnSave()
        {
            OnApplyTheme();
            _themeManager.ThemeChanged -= OnThemeChanged;
            CloseWindow?.Invoke(true);
        }

        private void OnCancel()
        {
            _themeManager.ThemeChanged -= OnThemeChanged;
            _themeManager.ApplyTheme(_originalTheme, _originalAccent);
            CloseWindow?.Invoke(false);
        }

        private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            _previewSettings.SelectedTheme = e.NewTheme;
            _previewSettings.AccentColor = e.NewAccent;
            RefreshPreviewBindings();
            UpdateSwatchSelection();
        }

        private static UserSettings CloneSettings(UserSettings source)
        {
            return new UserSettings
            {
                SelectedTheme = source.SelectedTheme,
                AccentColor = source.AccentColor,
                AnimationSpeed = source.AnimationSpeed,
                UiDensity = source.UiDensity,
                BackgroundStyle = source.BackgroundStyle,
                HighContrastEnabled = source.HighContrastEnabled,
                FocusIndicatorEnabled = source.FocusIndicatorEnabled,
                RenderMode = source.RenderMode,
                SafeModeEnabled = source.SafeModeEnabled
            };
        }

        public void Cleanup()
        {
            _themeManager.ThemeChanged -= OnThemeChanged;
            DiagnosticsVM.Cleanup();
        }
    }

    public sealed class AccentSwatchItem : ViewModelBase
    {
        private bool _isSelected;

        public string Hex { get; }
        public string Name { get; }
        public string AutomationName { get; }
        public Brush Brush { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public AccentSwatchItem(string hex, string name, string automationName)
        {
            Hex = hex;
            Name = name;
            AutomationName = automationName;
            Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }
}
