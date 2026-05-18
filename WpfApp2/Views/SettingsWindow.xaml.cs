using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Views
{
    public partial class SettingsWindow : ModernWindow
    {
        private readonly List<AccentColorOption> _accentOptions = new List<AccentColorOption>
        {
            new AccentColorOption { Name = "أزرق",    Hex = "#3A86FF" },
            new AccentColorOption { Name = "سماوي",   Hex = "#00B4D8" },
            new AccentColorOption { Name = "أخضر",    Hex = "#2ED573" },
            new AccentColorOption { Name = "بنفسجي",  Hex = "#9B59B6" },
            new AccentColorOption { Name = "برتقالي", Hex = "#FF6B35" },
            new AccentColorOption { Name = "وردي",    Hex = "#FF4D8D" },
            new AccentColorOption { Name = "أصفر",    Hex = "#FFA502" },
            new AccentColorOption { Name = "رمادي",   Hex = "#6C757D" },
        };

        private string _selectedAccent;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadCurrentSettings()
        {
            var settings = ThemeManager.GetCurrentSettings();
            _selectedAccent = settings.AccentColor;

            foreach (var opt in _accentOptions)
                opt.IsSelected = opt.Hex.Equals(_selectedAccent, System.StringComparison.OrdinalIgnoreCase);

            ThemeManager.ChangeAccent(_selectedAccent);
        }

        private void AccentColor_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn?.Tag is string hex)
            {
                _selectedAccent = hex;

                foreach (var opt in _accentOptions)
                    opt.IsSelected = opt.Hex.Equals(hex, System.StringComparison.OrdinalIgnoreCase);

                ThemeManager.ChangeAccent(hex);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var theme = ThemeType.FluentDarkBlue;

            ThemeManager.ApplyTheme(theme);
            ThemeManager.ChangeAccent(_selectedAccent);

            ThemeManager.SaveSettings();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var settings = ThemeManager.GetCurrentSettings();
            ThemeManager.ChangeAccent(settings.AccentColor);

            DialogResult = false;
            Close();
        }
    }

    public class AccentColorOption : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Hex { get; set; }

        public SolidColorBrush Brush =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(Hex));

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}