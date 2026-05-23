using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private string _selectedTheme = "Dark";
        private string _selectedAccent = "#3A86FF";
        private string _originalTheme;
        private string _originalAccent;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    OnPropertyChanged(nameof(IsDarkSelected));
                    OnPropertyChanged(nameof(IsLightSelected));
                    OnPropertyChanged(nameof(IsCustomSelected));
                    OnPropertyChanged(nameof(AccentSwatchesVisible));
                }
            }
        }

        public string SelectedAccent
        {
            get => _selectedAccent;
            set => SetProperty(ref _selectedAccent, value);
        }

        public bool IsDarkSelected => _selectedTheme == "Dark";
        public bool IsLightSelected => _selectedTheme == "Light";
        public bool IsCustomSelected => _selectedTheme == "Custom";
        public bool AccentSwatchesVisible => _selectedTheme == "Custom";

        public ObservableCollection<AccentSwatchItem> AccentSwatches { get; } = new();

        public ICommand ThemeCardCommand { get; }
        public ICommand AccentSwatchCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool?>? CloseWindow { get; set; }

        public SettingsViewModel(IServiceContainer container)
        {
            _themeManager = container.Resolve<ThemeManager>();

            ThemeCardCommand = new RelayCommand(param => OnThemeCardClick(param as string));
            AccentSwatchCommand = new RelayCommand(param => OnAccentSwatchClick(param as string));
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);

            LoadCurrentSettings();
            InitializeSwatches();

            _themeManager.ThemeChanged += OnThemeChanged;
        }

        private void LoadCurrentSettings()
        {
            _selectedTheme = _themeManager.CurrentTheme;
            _selectedAccent = _themeManager.CurrentAccentColor;
            _originalTheme = _selectedTheme;
            _originalAccent = _selectedAccent;

            OnPropertyChanged(nameof(SelectedTheme));
            OnPropertyChanged(nameof(SelectedAccent));
            OnPropertyChanged(nameof(IsDarkSelected));
            OnPropertyChanged(nameof(IsLightSelected));
            OnPropertyChanged(nameof(IsCustomSelected));
            OnPropertyChanged(nameof(AccentSwatchesVisible));
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
            SelectedTheme = themeName;
            var accent = themeName == "Custom" ? _selectedAccent : null;
            _themeManager.ApplyTheme(themeName, accent);
        }

        private void OnAccentSwatchClick(string? hex)
        {
            if (string.IsNullOrEmpty(hex)) return;
            SelectedAccent = hex;
            _themeManager.ApplyTheme("Custom", hex);
            UpdateSwatchSelection();
        }

        private void UpdateSwatchSelection()
        {
            foreach (var swatch in AccentSwatches)
                swatch.IsSelected = swatch.Hex.Equals(_selectedAccent, StringComparison.OrdinalIgnoreCase);
        }

        private void OnSave()
        {
            _themeManager.ThemeChanged -= OnThemeChanged;
            _themeManager.SaveCurrentTheme();
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
            SelectedTheme = e.NewTheme;
            SelectedAccent = e.NewAccent;
            UpdateSwatchSelection();
        }

        public void Cleanup()
        {
            _themeManager.ThemeChanged -= OnThemeChanged;
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
