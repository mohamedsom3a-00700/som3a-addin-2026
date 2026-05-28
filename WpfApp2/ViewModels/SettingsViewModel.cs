using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private readonly SettingsPersistenceService _settingsService;
        private readonly SettingsRegistry _registry;
        private readonly SettingsValidator _validator;
        private readonly IEventBus _eventBus;

        private UserSettings _currentSettings;
        private UserSettings _previewSettings;
        private SettingsCategory? _selectedCategory;
        private bool _isDirty;
        private bool _isPreviewActive;
        private string _originalTheme;
        private string _originalAccent;
        private object? _currentPanel;
        private SettingsSectionViewModel? _selectedSection;

        public ObservableCollection<SettingsCategory> Categories { get; } = new();
        public ObservableCollection<AccentSwatchItem> AccentSwatches { get; } = new();
        public ObservableCollection<SettingsSectionViewModel> DynamicSections { get; } = new();
        public ObservableCollection<AccentVariantItem> AccentVariants { get; } = new();
        public ObservableCollection<Services.FontFamilyInfo> AvailableFonts { get; } = new();
        public ObservableCollection<WallpaperItem> WallpaperImages { get; } = new();

        private Color _selectedCustomColor = Color.FromRgb(58, 134, 255);
        public Color SelectedCustomColor
        {
            get => _selectedCustomColor;
            set
            {
                if (SetProperty(ref _selectedCustomColor, value))
                {
                    var hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
                    _hexColorText = hex;
                    OnPropertyChanged(nameof(HexColorText));
                    AccentPreviewBrush = new SolidColorBrush(value);
                    OnPropertyChanged(nameof(AccentPreviewBrush));
                    ApplyAccentWithDebounce(hex);
                }
            }
        }

        private string _hexColorText = "#3A86FF";
        public string HexColorText
        {
            get => _hexColorText;
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length < 7)
                    return;

                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(value);
                    if (SetProperty(ref _hexColorText, value))
                    {
                        _selectedCustomColor = color;
                        OnPropertyChanged(nameof(SelectedCustomColor));
                        AccentPreviewBrush = new SolidColorBrush(color);
                        OnPropertyChanged(nameof(AccentPreviewBrush));
                        ApplyAccentWithDebounce(value);
                        UpdateAccentVariants(color);
                    }
                }
                catch { }
            }
        }

        private Brush _accentPreviewBrush = new SolidColorBrush(Color.FromRgb(58, 134, 255));
        public Brush AccentPreviewBrush
        {
            get => _accentPreviewBrush;
            set => SetProperty(ref _accentPreviewBrush, value);
        }

        private System.Windows.Threading.DispatcherTimer _accentDebounceTimer;
        private string _pendingAccentHex;

        private void ApplyAccentWithDebounce(string hex)
        {
            _pendingAccentHex = hex;
            if (_accentDebounceTimer == null)
            {
                _accentDebounceTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _accentDebounceTimer.Tick += (s, e) =>
                {
                    _accentDebounceTimer.Stop();
                    _themeManager.ApplyTheme("Custom", _pendingAccentHex);
                };
            }
            _accentDebounceTimer.Stop();
            _accentDebounceTimer.Start();
        }

        private void UpdateAccentVariants(Color baseColor)
        {
            AccentVariants.Clear();

            void AddVariant(string label, string suffix)
            {
                var color = Application.Current?.Resources[$"Accent.Color.{suffix}"] as Color?;
                if (color.HasValue)
                {
                    var brush = new SolidColorBrush(color.Value);
                    brush.Freeze();
                    AccentVariants.Add(new AccentVariantItem { Label = label, Brush = brush, Color = color.Value });
                }
            }

            AddVariant("Hover", "Hover");
            AddVariant("Pressed", "Pressed");
            AddVariant("Glow", "Glow");
            AddVariant("Border", "Border");
            AddVariant("Subtle", "Subtle");
        }

        private string _imageFileName = "";
        public string ImageFileName
        {
            get => _imageFileName;
            set => SetProperty(ref _imageFileName, value);
        }

        private string _imageValidationError = "";
        public string ImageValidationError
        {
            get => _imageValidationError;
            set => SetProperty(ref _imageValidationError, value);
        }

        private double _blurIntensity = 0.0;
        public double BlurIntensityPercent
        {
            get => _blurIntensity * 100.0;
            set
            {
                _blurIntensity = value / 100.0;
                OnPropertyChanged(nameof(BlurIntensityPercent));
                _themeManager.ApplyBackground(_imagePath, _blurEnabled ? _blurIntensity : 0.0);
            }
        }

        private bool _blurEnabled = false;
        public bool BlurEnabled
        {
            get => _blurEnabled;
            set
            {
                if (SetProperty(ref _blurEnabled, value))
                {
                    _themeManager.ApplyBackground(_imagePath, value ? _blurIntensity : 0.0);
                }
            }
        }

        private string _imagePath = "";
        private Services.FontFamilyInfo _selectedFont;
        public Services.FontFamilyInfo SelectedFont
        {
            get => _selectedFont;
            set
            {
                if (SetProperty(ref _selectedFont, value) && value != null)
                {
                    _themeManager.ApplyFont(value.FamilyName);
                }
            }
        }

        public ICommand SelectImageCommand { get; private set; }
        public ICommand ClearImageCommand { get; private set; }
        public ICommand SetWallpaperColorCommand { get; private set; }
        public ICommand SetWallpaperImageCommand { get; private set; }
        public ICommand ClearWallpaperCommand { get; private set; }
        public ICommand CustomBaseThemeCommand { get; private set; }

        private string _customBaseTheme = "Dark";
        public bool CustomBaseIsDark => _customBaseTheme == "Dark";
        public bool CustomBaseIsLight => _customBaseTheme == "Light";

        private void ExecuteSetWallpaperColor(object parameter)
        {
            if (parameter is string hexColor)
            {
                _imagePath = "";
                ImageFileName = "";
                ImageValidationError = "";
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hexColor);
                    var brush = new SolidColorBrush(color);
                    _themeManager.ApplyBackground("", 0.0);
                    if (Application.Current?.Windows.Count > 0)
                    {
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window is Controls.ModernWindow mw)
                            {
                                mw.Background = brush;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void ExecuteSetWallpaperImage(object parameter)
        {
            if (parameter is string imagePath)
            {
                _imagePath = imagePath;
                ImageFileName = System.IO.Path.GetFileName(imagePath);
                ImageValidationError = "";
                _themeManager.ApplyBackground(imagePath, _blurEnabled ? _blurIntensity : 0.0);
                Properties.Settings.Default.BackgroundImagePath = imagePath;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadWallpapers()
        {
            WallpaperImages.Clear();
            try
            {
                var candidates = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper"),
                    System.IO.Path.Combine(System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".", "Wallpaper"),
                    System.IO.Path.Combine(Environment.CurrentDirectory, "WpfApp2", "Wallpaper"),
                    System.IO.Path.Combine(Environment.CurrentDirectory, "Wallpaper"),
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                        System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))) ?? ".", 
                        "WpfApp2", "Wallpaper"),
                };

                string wallpaperDir = null;
                foreach (var dir in candidates)
                {
                    if (System.IO.Directory.Exists(dir))
                    {
                        wallpaperDir = dir;
                        break;
                    }
                }

                if (wallpaperDir == null) return;

                var extensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };
                foreach (var file in System.IO.Directory.GetFiles(wallpaperDir))
                {
                    var ext = System.IO.Path.GetExtension(file).ToLowerInvariant();
                    if (Array.IndexOf(extensions, ext) >= 0)
                    {
                        var thumb = CreateThumbnail(file, 240, 144);
                        WallpaperImages.Add(new WallpaperItem
                        {
                            ImagePath = file,
                            Name = System.IO.Path.GetFileNameWithoutExtension(file),
                            Thumbnail = thumb
                        });
                    }
                }
            }
            catch { }

            if (WallpaperImages.Count == 0)
            {
                WallpaperImages.Add(new WallpaperItem { Name = "No wallpapers found" });
            }
        }

        private static ImageSource CreateThumbnail(string path, int width, int height)
        {
            try
            {
                var bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path);
                bmp.DecodePixelWidth = width;
                bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        public sealed class WallpaperItem
        {
            public string ImagePath { get; set; } = "";
            public string Name { get; set; } = "";
            public ImageSource Thumbnail { get; set; }
        }

        private void ExecuteClearWallpaper(object parameter)
        {
            _imagePath = "";
            ImageFileName = "";
            ImageValidationError = "";
            _themeManager.ApplyBackground("", 0.0);
        }

        private void SetCustomBaseTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName)) return;
            _customBaseTheme = themeName;
            OnPropertyChanged(nameof(CustomBaseIsDark));
            OnPropertyChanged(nameof(CustomBaseIsLight));
            _previewSettings.SelectedTheme = "Custom";
            _themeManager.ApplyTheme(themeName, _previewSettings.AccentColor);
            if (!string.IsNullOrEmpty(_imagePath))
                _themeManager.ApplyBackground(_imagePath, _blurEnabled ? _blurIntensity : 0.0);
            RefreshPreviewBindings();
        }

        private void ExecuteSelectImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
                Title = "Select Background Image"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var fileInfo = new System.IO.FileInfo(dlg.FileName);
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        ImageValidationError = "Image must be 10MB or smaller";
                        return;
                    }

                    var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".bmp")
                    {
                        ImageValidationError = "Supported formats: PNG, JPG, JPEG, BMP";
                        return;
                    }

                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(dlg.FileName);
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.EndInit();

                    if (img.PixelWidth > 4096 || img.PixelHeight > 4096)
                    {
                        ImageValidationError = "Image dimensions must be 4096px or smaller";
                        return;
                    }

                    _imagePath = dlg.FileName;
                    ImageFileName = fileInfo.Name;
                    ImageValidationError = "";
                    _themeManager.ApplyBackground(_imagePath, BlurEnabled ? _blurIntensity : 0.0);
                }
                catch
                {
                    ImageValidationError = "Failed to load image. File may be corrupt.";
                }
            }
        }

        private void ExecuteClearImage()
        {
            _imagePath = "";
            ImageFileName = "";
            ImageValidationError = "";
            _themeManager.ApplyBackground("", 0.0);
        }

        private void LoadFonts()
        {
            AvailableFonts.Clear();
            var fonts = Services.FontEnumerator.GetSystemFonts();
            foreach (var font in fonts)
                AvailableFonts.Add(font);
        }

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

        public SettingsSectionViewModel? SelectedSection
        {
            get => _selectedSection;
            set => SetProperty(ref _selectedSection, value);
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

        // AI
        public bool IsAIEnabled
        {
            get => _previewSettings.IsAIEnabled;
            set
            {
                if (_previewSettings.IsAIEnabled != value)
                {
                    _previewSettings.IsAIEnabled = value;
                    AISettings.IsAIEnabled = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(IsAIEnabled));
                }
            }
        }

        public string AIProviderType
        {
            get => _previewSettings.AIProviderType;
            set
            {
                if (_previewSettings.AIProviderType != value)
                {
                    _previewSettings.AIProviderType = value;
                    AISettings.ProviderType = value == "Ollama" ? Services.AIProviderType.Ollama : Services.AIProviderType.Cloud;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AIProviderType));
                    OnPropertyChanged(nameof(IsCloudSelected));
                    OnPropertyChanged(nameof(IsOllamaSelected));
                    OnPropertyChanged(nameof(HasCloudApiKey));

                    if (value == "Cloud" && string.IsNullOrEmpty(AISettings.CloudApiKey))
                    {
                        // Cloud selected but no key — if local available, warn to switch
                        if (AISettings.IsLocalProviderDetected)
                            ShowApiKeyWarning = true;
                    }
                    else
                    {
                        ShowApiKeyWarning = false;
                    }
                }
            }
        }

        public bool IsCloudSelected => AIProviderType == "Cloud";
        public bool IsOllamaSelected => AIProviderType == "Ollama";
        public bool HasCloudApiKey => AISettings.HasApiKey;

        public string AICloudApiKey
        {
            get => _previewSettings.AICloudApiKey;
            set
            {
                if (_previewSettings.AICloudApiKey != value)
                {
                    _previewSettings.AICloudApiKey = value;
                    AISettings.CloudApiKey = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AICloudApiKey));
                }
            }
        }

        public string AICloudMainModel
        {
            get => _previewSettings.AICloudMainModel;
            set
            {
                if (_previewSettings.AICloudMainModel != value)
                {
                    _previewSettings.AICloudMainModel = value;
                    AISettings.CloudMainModel = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AICloudMainModel));
                }
            }
        }

        public string AICloudSubModel
        {
            get => _previewSettings.AICloudSubModel;
            set
            {
                if (_previewSettings.AICloudSubModel != value)
                {
                    _previewSettings.AICloudSubModel = value;
                    AISettings.CloudSubModel = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AICloudSubModel));
                }
            }
        }

        public string AIOllamaEndpoint
        {
            get => _previewSettings.AIOllamaEndpoint;
            set
            {
                if (_previewSettings.AIOllamaEndpoint != value)
                {
                    _previewSettings.AIOllamaEndpoint = value;
                    AISettings.OllamaEndpoint = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AIOllamaEndpoint));
                }
            }
        }

        public string AIOllamaModel
        {
            get => _previewSettings.AIOllamaModel;
            set
            {
                if (_previewSettings.AIOllamaModel != value)
                {
                    _previewSettings.AIOllamaModel = value;
                    AISettings.OllamaModel = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AIOllamaModel));
                }
            }
        }

        // Local provider detection
        private bool _isDetectionScanning;
        public bool IsDetectionScanning
        {
            get => _isDetectionScanning;
            set => SetProperty(ref _isDetectionScanning, value);
        }

        private string _detectionStatusText = "";
        public string DetectionStatusText
        {
            get => _detectionStatusText;
            set => SetProperty(ref _detectionStatusText, value);
        }

        private int _detectedProviderCount;
        public int DetectedProviderCount
        {
            get => _detectedProviderCount;
            set
            {
                if (SetProperty(ref _detectedProviderCount, value))
                {
                    OnPropertyChanged(nameof(HasDetectedProviders));
                    OnPropertyChanged(nameof(HasMultipleLocalProviders));
                    OnPropertyChanged(nameof(ShowLocalProviderSelector));
                }
            }
        }

        public bool HasDetectedProviders => DetectedProviderCount > 0;
        public bool HasMultipleLocalProviders => DetectedProviderCount > 1;
        public bool ShowLocalProviderSelector => HasMultipleLocalProviders;

        public ObservableCollection<LocalProviderInfo> DetectedLocalProviders { get; } = new();

        private LocalProviderInfo? _selectedLocalProvider;
        public LocalProviderInfo? SelectedLocalProvider
        {
            get => _selectedLocalProvider;
            set
            {
                if (SetProperty(ref _selectedLocalProvider, value) && value != null)
                {
                    AIOllamaEndpoint = value.Endpoint;
                    AIOllamaModel = value.DefaultModel;
                    AISettings.ApplyLocalProviderSelection(value);
                    PopulateLocalModels(value);
                }
            }
        }

        private void PopulateLocalModels(LocalProviderInfo provider)
        {
            AvailableLocalModels.Clear();
            foreach (var m in provider.AvailableModels)
                AvailableLocalModels.Add(m);
            if (AvailableLocalModels.Count > 0)
            {
                AIOllamaSubModel = provider.FallbackModel;
            }
        }

        private bool _showApiKeyWarning;
        public bool ShowApiKeyWarning
        {
            get => _showApiKeyWarning;
            set => SetProperty(ref _showApiKeyWarning, value);
        }

        public string AIOllamaSubModel
        {
            get => _previewSettings.AIOllamaSubModel;
            set
            {
                if (_previewSettings.AIOllamaSubModel != value)
                {
                    _previewSettings.AIOllamaSubModel = value;
                    AISettings.OllamaSubModel = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(AIOllamaSubModel));
                }
            }
        }

        public ObservableCollection<string> AvailableLocalModels { get; } = new();

        public ObservableCollection<string> AvailableModels { get; } = new()
        {
            "gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo",
            "claude-3-opus", "claude-3-sonnet", "claude-3-haiku", "claude-2.1",
            "deepseek-chat", "deepseek-coder",
            "gemini-pro", "gemini-1.5-pro", "gemini-1.5-flash"
        };

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
        public ICommand ToggleAICommand { get; }
        public ICommand ToggleAIProviderCommand { get; }
        public ICommand RefreshModelsCommand { get; }
        public ICommand SwitchToLocalAICommand { get; }

        public Action<bool?>? CloseWindow { get; set; }

        public DiagnosticsViewModel DiagnosticsVM { get; }

        public SettingsViewModel(ThemeManager themeManager, SettingsPersistenceService settingsService, DiagnosticsViewModel diagnosticsVm,
            SettingsRegistry registry, SettingsValidator validator, IEventBus eventBus)
        {
            _themeManager = themeManager;
            _settingsService = settingsService;
            _registry = registry;
            _validator = validator;
            _eventBus = eventBus;

            _currentSettings = _settingsService.LoadSettings();
            _previewSettings = CloneSettings(_currentSettings);

            _originalTheme = _currentSettings.SelectedTheme;
            _originalAccent = _currentSettings.AccentColor;

            InitializeCategories();
            InitializeSwatches();
            LoadDynamicSections();

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
            SelectImageCommand = new RelayCommand(_ => ExecuteSelectImage());
            ClearImageCommand = new RelayCommand(_ => ExecuteClearImage());
            SetWallpaperColorCommand = new RelayCommand(param => ExecuteSetWallpaperColor(param));
            SetWallpaperImageCommand = new RelayCommand(param => ExecuteSetWallpaperImage(param));
            ClearWallpaperCommand = new RelayCommand(_ => ExecuteClearWallpaper(""));
            CustomBaseThemeCommand = new RelayCommand(param => SetCustomBaseTheme(param as string));
            ToggleAICommand = new RelayCommand(OnToggleAI);
            ToggleAIProviderCommand = new RelayCommand(param => OnToggleAIProvider(param as string));
            RefreshModelsCommand = new RelayCommand(async _ => await RefreshModelsAsync());
            SwitchToLocalAICommand = new RelayCommand(_ => OnSwitchToLocalAI());

            // Sync initial AI settings
            AISettings.IsAIEnabled = _previewSettings.IsAIEnabled;
            AISettings.ProviderType = _previewSettings.AIProviderType == "Ollama" ? Services.AIProviderType.Ollama : Services.AIProviderType.Cloud;
            AISettings.CloudApiKey = _previewSettings.AICloudApiKey;
            AISettings.CloudMainModel = _previewSettings.AICloudMainModel;
            AISettings.CloudSubModel = _previewSettings.AICloudSubModel;
            AISettings.OllamaEndpoint = _previewSettings.AIOllamaEndpoint;
            AISettings.OllamaModel = _previewSettings.AIOllamaModel;
            AISettings.OllamaSubModel = _previewSettings.AIOllamaSubModel;

            // Start local provider detection
            _ = DetectLocalProvidersAsync();

            _themeManager.ThemeChanged += OnThemeChanged;
            LoadFonts();
            LoadWallpapers();
        }

        private void LoadDynamicSections()
        {
            DynamicSections.Clear();

            var categories = _registry.GetAllCategories();
            foreach (var category in categories)
            {
                var sections = _registry.GetSectionsByCategory(category);
                foreach (var section in sections)
                {
                    var sectionVm = new SettingsSectionViewModel(section, _validator);
                    DynamicSections.Add(sectionVm);
                }
            }
        }

        public void RefreshDynamicSections()
        {
            LoadDynamicSections();
        }

        private void InitializeCategories()
        {
            Categories.Add(new SettingsCategory
            {
                Id = "appearance",
                DisplayName = "Appearance",
                Icon = "Palette",
                PanelType = typeof(Views.AppearancePanel),
                Order = 1
            });
            Categories.Add(new SettingsCategory
            {
                Id = "performance",
                DisplayName = "Performance",
                Icon = "Speedometer",
                PanelType = typeof(Views.PerformancePanel),
                Order = 2
            });
            Categories.Add(new SettingsCategory
            {
                Id = "accessibility",
                DisplayName = "Accessibility",
                Icon = "Human",
                PanelType = typeof(Views.AccessibilityPanel),
                Order = 3
            });
            Categories.Add(new SettingsCategory
            {
                Id = "diagnostics",
                DisplayName = "Diagnostics",
                Icon = "ChartBar",
                PanelType = typeof(Views.DiagnosticsPanel),
                Order = 4
            });
            Categories.Add(new SettingsCategory
            {
                Id = "excel",
                DisplayName = "Excel",
                Icon = "FileExcel",
                PanelType = typeof(Views.ExcelPanel),
                Order = 5
            });
            Categories.Add(new SettingsCategory
            {
                Id = "plugins",
                DisplayName = "Plugins",
                Icon = "Puzzle",
                PanelType = typeof(Views.PluginsPanel),
                Order = 6
            });
            Categories.Add(new SettingsCategory
            {
                Id = "wbs",
                DisplayName = "WBS Engine",
                Icon = "Sitemap",
                PanelType = typeof(Views.WbsPanel),
                Order = 7
            });
            Categories.Add(new SettingsCategory
            {
                Id = "ai",
                DisplayName = "AI",
                Icon = "Robot",
                PanelType = typeof(Views.AIPanel),
                Order = 8
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

        private void OnToggleAI()
        {
            IsAIEnabled = !IsAIEnabled;
        }

        private void OnToggleAIProvider(string? providerType)
        {
            if (!string.IsNullOrEmpty(providerType))
                AIProviderType = providerType;
        }

        private void OnSwitchToLocalAI()
        {
            var providers = AISettings.DetectedLocalProviders;
            if (providers.Count > 0)
            {
                var first = providers[0];
                AIProviderType = "Ollama";
                AIOllamaEndpoint = first.Endpoint;
                AIOllamaModel = first.DefaultModel;
                AIOllamaSubModel = first.FallbackModel;
                AISettings.ApplyLocalProviderSelection(first);
                ShowApiKeyWarning = false;
            }
        }

        private async Task DetectLocalProvidersAsync()
        {
            IsDetectionScanning = true;
            DetectionStatusText = "Scanning for local AI providers...";

            try
            {
                var detected = await Task.Run(() => LocalProviderDetector.Detect());
                AISettings.DetectedLocalProviders = detected;
                DetectedProviderCount = detected.Count;

                DetectedLocalProviders.Clear();
                foreach (var p in detected)
                    DetectedLocalProviders.Add(p);

                if (detected.Count == 0)
                {
                    DetectionStatusText = "No local AI providers detected.";
                }
                else
                {
                    DetectionStatusText = detected.Count == 1
                        ? $"Detected: {detected[0].DisplayName}"
                        : $"Detected {detected.Count} local providers. Select one below.";

                    var savedId = AISettings.SelectedLocalProviderId;
                    var preferred = detected.FirstOrDefault(p => p.Id == savedId) ?? detected[0];
                    AISettings.ApplyLocalProviderSelection(preferred);

                    // Populate local model combos
                    PopulateLocalModels(preferred);
                    SelectedLocalProvider = preferred;

                    // Flow: if Cloud has no API key, auto-use local AI
                    if (string.IsNullOrEmpty(AISettings.CloudApiKey))
                    {
                        if (detected.Count == 1)
                        {
                            OnSwitchToLocalAI();
                        }
                        else
                        {
                            ShowApiKeyWarning = true;
                        }
                    }
                }
            }
            catch
            {
                DetectionStatusText = "Failed to detect local providers.";
                DetectedProviderCount = 0;
            }
            finally
            {
                IsDetectionScanning = false;
            }
        }

        private async Task RefreshModelsAsync()
        {
            var apiKey = AICloudApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                System.Windows.MessageBox.Show("Enter an API key first, then refresh models.", "API Key Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var response = await client.GetAsync("https://api.openai.com/v1/models");
                if (!response.IsSuccessStatusCode)
                {
                    System.Windows.MessageBox.Show($"Failed to fetch models. HTTP {(int)response.StatusCode}. Check your API key.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                var json = await response.Content.ReadAsStringAsync();
                var root = Newtonsoft.Json.Linq.JObject.Parse(json);
                var models = root["data"]?
                    .Select(m => (string?)m["id"])
                    .Where(id => id != null && !id.Contains("realtime") && !id.Contains("instruct"))
                    .OrderBy(id => id)
                    .ToList();

                if (models.Count > 0)
                {
                    AvailableModels.Clear();
                    foreach (var m in models)
                        AvailableModels.Add(m!);
                    System.Windows.MessageBox.Show($"Loaded {models.Count} models from OpenAI.", "Models Updated", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error fetching models: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
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

        private async void OnExportSettings()
        {
            try
            {
                var filePath = GetExportFilePath();
                if (string.IsNullOrEmpty(filePath)) return;
                _settingsService.ExportSettings(_currentSettings, filePath);
                await _settingsService.ExportSnapshotAsync(filePath, null);
                ToastService.Success("Settings exported successfully.");
            }
            catch (Exception ex)
            {
                ToastService.Error($"Export failed: {ex.Message}");
            }
        }

        private async void OnImportSettings()
        {
            var filePath = GetImportFilePath();
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                var bundle = await _settingsService.ImportSnapshotAsync(filePath, null);
                if (bundle != null)
                {
                    var pluginCount = bundle.Plugins.Count;
                    var sectionCount = bundle.Plugins.Sum(p => p.Value?.SectionKeys.Count ?? 0);

                    if (sectionCount > 0)
                    {
                        ToastService.Success($"Settings imported: {pluginCount} plugins, {sectionCount} sections restored.");
                        RefreshDynamicSections();
                    }
                }

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

        private async void OnSave()
        {
            OnApplyTheme();

            foreach (var sectionVm in DynamicSections)
            {
                var pluginId = sectionVm.PluginId;
                var doc = new PluginSettingsDocument
                {
                    Version = 1,
                    LastModified = DateTime.UtcNow,
                    SectionKeys = new System.Collections.Generic.List<string> { sectionVm.Id }
                };

                foreach (var control in sectionVm.Controls)
                {
                    doc.Values[control.Key] = control.CurrentValue;

                    if (control.IsEncrypted && control.CurrentValue is string secret && !string.IsNullOrEmpty(secret))
                    {
                        var bytes = System.Text.Encoding.UTF8.GetBytes(secret);
                        await _settingsService.SaveEncryptedValueAsync(pluginId, $"{sectionVm.Id}.{control.Key}", bytes);
                        doc.Values[control.Key] = null;
                    }

                    _registry.UpdateSettingValue(sectionVm.Id, control.Key, control.CurrentValue);

                    var isSecret = control.IsEncrypted;
                    _eventBus.Publish(new SettingsChangedEvent
                    {
                        ModuleId = pluginId,
                        SectionId = sectionVm.Id,
                        SettingKey = control.Key,
                        OldValue = isSecret ? "***" : null,
                        NewValue = isSecret ? "***" : control.CurrentValue,
                        ChangedAt = DateTime.UtcNow
                    });
                }

                await _settingsService.SavePluginSettingsAsync(pluginId, doc);
            }

            _registry.MarkClean();
            IsDirty = false;

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
                SafeModeEnabled = source.SafeModeEnabled,
                IsAIEnabled = source.IsAIEnabled,
                AIProviderType = source.AIProviderType,
                AICloudApiKey = source.AICloudApiKey,
                AICloudMainModel = source.AICloudMainModel,
                AICloudSubModel = source.AICloudSubModel,
                AIOllamaEndpoint = source.AIOllamaEndpoint,
                AIOllamaModel = source.AIOllamaModel,
                AIOllamaSubModel = source.AIOllamaSubModel
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

    public sealed class AccentVariantItem
    {
        public string Label { get; set; } = "";
        public Brush Brush { get; set; } = Brushes.White;
        public Color Color { get; set; }
    }
}
