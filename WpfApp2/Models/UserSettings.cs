using System;

namespace Som3a_WPF_UI.Models
{
    public sealed class UserSettings
    {
        public string SelectedTheme { get; set; } = "Dark";
        public string AccentColor { get; set; } = "#3A86FF";
        public string AnimationSpeed { get; set; } = "Full";
        public string UiDensity { get; set; } = "Normal";
        public string BackgroundStyle { get; set; } = "Gradient";
        public bool HighContrastEnabled { get; set; } = false;
        public bool FocusIndicatorEnabled { get; set; } = true;
        public string RenderMode { get; set; } = "Auto";
        public bool SafeModeEnabled { get; set; } = false;
        public string WBSCodeMode { get; set; } = "Numeric";
        public int WBSMinAlphaLength { get; set; } = 3;
        public string WBSExportStyle { get; set; } = "Default";
    }
}
