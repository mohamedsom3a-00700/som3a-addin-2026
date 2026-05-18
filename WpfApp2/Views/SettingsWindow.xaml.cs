using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
<<<<<<< HEAD
<<<<<<< HEAD
using System.Windows.Shapes;
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
using System.Windows.Shapes;
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
using Som3a_WPF_UI.Controls;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Views
{
    public partial class SettingsWindow : ModernWindow
    {
        private string _selectedTheme = "Dark";
        private string _selectedAccent = "#3A86FF";
<<<<<<< HEAD
<<<<<<< HEAD
        private string _originalTheme;
        private string _originalAccent;
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
        private string _originalTheme;
        private string _originalAccent;
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
            _selectedTheme = ThemeManager.Instance.CurrentTheme;
            _selectedAccent = ThemeManager.Instance.CurrentAccentColor;
<<<<<<< HEAD
<<<<<<< HEAD
            _originalTheme = _selectedTheme;
            _originalAccent = _selectedAccent;
<<<<<<< HEAD
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
            _originalTheme = _selectedTheme;
            _originalAccent = _selectedAccent;
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
=======
>>>>>>> cb7ce28 (fix: resolve merge conflicts and hardcoded colors in theme engine)

            UpdateCardSelection();
            UpdateSwatchSelection();

            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _selectedTheme = e.NewTheme;
                _selectedAccent = e.NewAccent;
                UpdateCardSelection();
                UpdateSwatchSelection();
            });
        }

        private void UpdateCardSelection()
        {
            CardDark.IsChecked = _selectedTheme == "Dark";
            CardLight.IsChecked = _selectedTheme == "Light";
            CardCustom.IsChecked = _selectedTheme == "Custom";

            AccentSwatchesPanel.Visibility = _selectedTheme == "Custom" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateSwatchSelection()
        {
            var swatchElements = new[] {
                (FindName("SwatchBlue") as Ellipse, "#3A86FF"),
                (FindName("SwatchGreen") as Ellipse, "#2ED573"),
                (FindName("SwatchPurple") as Ellipse, "#A855F7"),
                (FindName("SwatchOrange") as Ellipse, "#FFA502"),
                (FindName("SwatchPink") as Ellipse, "#EC4899"),
                (FindName("SwatchTeal") as Ellipse, "#14B8A6"),
                (FindName("SwatchRed") as Ellipse, "#EF4444"),
                (FindName("SwatchCyan") as Ellipse, "#06B6D4"),
            };

            foreach (var (ellipse, hex) in swatchElements)
            {
                if (ellipse == null) continue;
                var isSelected = hex.Equals(_selectedAccent, System.StringComparison.OrdinalIgnoreCase);
                ellipse.Style = (Style)FindResource(isSelected ? "AccentSwatchSelected" : "AccentSwatchInteractive");
            }
        }

        private void ThemeCard_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
            ApplyThemeCardSelection(sender);
        }

        private void ThemeCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                ApplyThemeCardSelection(sender);
                e.Handled = true;
            }
        }

        private void ApplyThemeCardSelection(object sender)
        {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
            var border = sender as Border;
            if (border?.Tag is string themeName)
=======
            var btn = sender as ToggleButton;
            if (btn?.Tag is string themeName)
>>>>>>> 6075fc8 (feat: refactor theme card selection to ToggleButton with proper selection state)
            {
                _selectedTheme = themeName;

                var accent = _selectedTheme == "Custom" ? _selectedAccent : null;
                ThemeManager.Instance.ApplyTheme(themeName, accent);

                CardDark.IsChecked = themeName == "Dark";
                CardLight.IsChecked = themeName == "Light";
                CardCustom.IsChecked = themeName == "Custom";
            }
        }

        private void AccentSwatch_Click(object sender, MouseButtonEventArgs e)
        {
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
=======
>>>>>>> cb7ce28 (fix: resolve merge conflicts and hardcoded colors in theme engine)
            ApplySwatchSelection(sender);
        }

        private void AccentSwatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                ApplySwatchSelection(sender);
                e.Handled = true;
            }
        }

        private void ApplySwatchSelection(object sender)
        {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
=======
>>>>>>> cb7ce28 (fix: resolve merge conflicts and hardcoded colors in theme engine)
            var ellipse = sender as Ellipse;
            if (ellipse?.Tag is string hex)
            {
                _selectedAccent = hex;
                ThemeManager.Instance.ApplyTheme("Custom", hex);
                UpdateSwatchSelection();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.SaveCurrentTheme();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            ThemeManager.Instance.ApplyTheme(_originalTheme, _originalAccent);
=======
>>>>>>> c8d3899 ([Spec Kit] Phase 1-3: Effects library + Theme system + Theme cards MVP)
=======
            ThemeManager.Instance.ApplyTheme(_originalTheme, _originalAccent);
>>>>>>> 9d81f77 (feat: Fluent Theme Engine Phase 2 complete — build fixes + Phase 4-8)
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            base.OnClosed(e);
        }
    }
}