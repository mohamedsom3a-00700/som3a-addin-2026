using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Som3a_WPF_UI.Services
{
    public class ArabicFontManager
    {
        private static readonly Lazy<ArabicFontManager> _instance =
            new Lazy<ArabicFontManager>(() => new ArabicFontManager());

        public static ArabicFontManager Instance => _instance.Value;

        private string _currentArabicFont = "Cairo";

        public string CurrentArabicFont => _currentArabicFont;

        public void ResetFont()
        {
            try
            {
                _currentArabicFont = "Segoe UI";
                var font = new FontFamily("Segoe UI, Arial");
                ApplyArabicFontToShell(font);
            }
            catch { }
        }

        public IReadOnlyList<FontOption> AvailableFonts { get; } = new[]
        {
            new FontOption { DisplayName = "Cairo", FontFamily = "Cairo", PreviewText = "مرحباً", IsBundled = true },
            new FontOption { DisplayName = "IBM Plex Sans Arabic", FontFamily = "IBM Plex Sans Arabic", PreviewText = "مرحباً", IsBundled = true },
            new FontOption { DisplayName = "Segoe UI", FontFamily = "Segoe UI", PreviewText = "مرحباً", IsBundled = false }
        };

        public void SetArabicFont(string fontFamily)
        {
            if (string.IsNullOrEmpty(fontFamily))
                return;

            try
            {
                var font = new FontFamily(fontFamily + ", Segoe UI, Arial");
                if (font != null)
                {
                    _currentArabicFont = fontFamily;
                    ApplyArabicFontToShell(font);
                }
            }
            catch
            {
            }
        }

        public string[] GetFontFallbackChain(string languageCode)
        {
            if (languageCode == null)
                return new[] { "Segoe UI", "Arial" };

            if (languageCode.StartsWith("ar"))
            {
                return new[] { _currentArabicFont, "Segoe UI", "Arial" };
            }

            return new[] { "Segoe UI", "Arial" };
        }

        private static void ApplyArabicFontToShell(FontFamily fontFamily)
        {
            try
            {
                SetResource("CustomFontFamily", fontFamily);
                SetResource("FontFamilyPrimary", fontFamily);

                var shellWindows = Application.Current.Windows;
                foreach (Window window in shellWindows)
                {
                    if (window is Controls.Shell.ShellWindow shell)
                    {
                        shell.FontFamily = fontFamily;
                    }
                }

                foreach (Window window in shellWindows)
                {
                    ForceFontRefresh(window);
                }
            }
            catch
            {
            }
        }

        private static void SetResource(string key, object value)
        {
            if (Application.Current.Resources.Contains(key))
                Application.Current.Resources.Remove(key);
            Application.Current.Resources.Add(key, value);
        }

        private static void ForceFontRefresh(DependencyObject element)
        {
            if (element == null) return;
            var localValues = element.GetLocalValueEnumerator();
            while (localValues.MoveNext())
            {
                var entry = localValues.Current;
                if (entry.Property == TextBlock.FontFamilyProperty ||
                    entry.Property == Control.FontFamilyProperty)
                {
                    var expr = BindingOperations.GetBindingExpression(element, entry.Property);
                    if (expr != null)
                        expr.UpdateTarget();
                }
            }
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
                ForceFontRefresh(VisualTreeHelper.GetChild(element, i));
        }
    }

    public class FontOption
    {
        public string DisplayName { get; set; }
        public string FontFamily { get; set; }
        public string PreviewText { get; set; }
        public bool IsBundled { get; set; }
    }
}
