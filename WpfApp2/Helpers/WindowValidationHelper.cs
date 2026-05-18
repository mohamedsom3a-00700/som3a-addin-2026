using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Som3a_WPF_UI.Helpers
{
    public static class WindowValidationHelper
    {
        public static readonly List<ValidationIssue> ValidationIssues = new();

        public static void ValidateAllWindows()
        {
            ValidationIssues.Clear();

            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return;

            var windowTypes = assembly.GetTypes()
                .Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var windowType in windowTypes)
            {
                ValidateWindowType(windowType);
            }

            if (ValidationIssues.Count > 0)
            {
                var message = $"Window Validation Issues:\n\n" +
                    string.Join("\n", ValidationIssues.Select(i => $"[{i.Severity}] {i.Message}"));

                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        private static void ValidateWindowType(Type windowType)
        {
            if (windowType.Name == "Window" || windowType.Name == "ModernWindow")
                return;

            var baseType = windowType.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Window" &&
                    baseType.Namespace != null &&
                    baseType.Namespace.Contains("System.Windows"))
                {
                    if (!typeof(Controls.ModernWindow).IsAssignableFrom(windowType))
                    {
                        ValidationIssues.Add(new ValidationIssue
                        {
                            Severity = "WARNING",
                            Message = $"{windowType.Name} should inherit from ModernWindow instead of Window"
                        });
                    }
                    break;
                }

                if (baseType.Name == "ModernWindow" && baseType.Namespace == "Som3a_WPF_UI.Controls")
                    break;

                baseType = baseType.BaseType;
            }

            ValidateWindowAttributes(windowType);
        }

        private static void ValidateWindowAttributes(Type windowType)
        {
            var props = windowType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var allowsTransparencyProp = props.FirstOrDefault(p => p.Name == "AllowsTransparency");
            if (allowsTransparencyProp != null)
            {
                var defaultValue = allowsTransparencyProp.GetValue(null);
                if (defaultValue is true)
                {
                    ValidationIssues.Add(new ValidationIssue
                    {
                        Severity = "WARNING",
                        Message = $"{windowType.Name} has AllowsTransparency=True by default"
                    });
                }
            }
        }

        public static bool IsVstoMode()
        {
            try
            {
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                return processName.IndexOf("EXCEL", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        public static void LogDpiInfo()
        {
            try
            {
                var dpi = DpiHelper.GetSystemDpi();
                var scale = dpi / 96.0;
                System.Diagnostics.Debug.WriteLine($"DPI: {dpi}, Scale: {scale:P0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DPI detection failed: {ex.Message}");
            }
        }
    }

    public class ValidationIssue
    {
        public string Severity { get; set; } = "INFO";
        public string Message { get; set; } = "";
    }
}