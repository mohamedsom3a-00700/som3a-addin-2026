using Som3a_WPF_UI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public sealed class MigrationTracker
    {
        private static readonly string TrackerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "migration-tracker.json");

        public HashSet<string> MigratedCategories { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public static MigrationTracker Load()
        {
            try
            {
                if (File.Exists(TrackerPath))
                {
                    var json = File.ReadAllText(TrackerPath);
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<MigrationTracker>(json) ?? new MigrationTracker();
                }
            }
            catch
            {
            }

            return new MigrationTracker();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(TrackerPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(TrackerPath, json);
            }
            catch
            {
            }
        }
    }

    public sealed class SettingsMigrationService
    {
        private readonly SettingsRegistry _registry;
        private readonly MigrationTracker _tracker;

        public HashSet<string> MigratedCategories => _tracker.MigratedCategories;

        public SettingsMigrationService()
        {
            _registry = SettingsRegistry.Instance;
            _tracker = MigrationTracker.Load();
        }

        public async Task MigrateCategoryAsync(string category)
        {
            if (_tracker.MigratedCategories.Contains(category))
                return;

            switch (category.ToLowerInvariant())
            {
                case "theme":
                    await MigrateThemeSettingsAsync();
                    break;
                case "accessibility":
                    await MigrateAccessibilitySettingsAsync();
                    break;
                case "performance":
                    await MigratePerformanceSettingsAsync();
                    break;
                case "diagnostics":
                    await MigrateDiagnosticsSettingsAsync();
                    break;
                case "excel":
                    await MigrateExcelSettingsAsync();
                    break;
                default:
                    throw new ArgumentException($"Unknown category: {category}", nameof(category));
            }

            _tracker.MigratedCategories.Add(category);
            _tracker.Save();
        }

        public async Task MigrateAllUnmigratedAsync()
        {
            var categories = new[] { "Theme", "Accessibility", "Performance", "Diagnostics", "Excel" };
            foreach (var category in categories)
            {
                if (!_tracker.MigratedCategories.Contains(category))
                    await MigrateCategoryAsync(category);
            }
        }

        private Task MigrateThemeSettingsAsync()
        {
            var section = new SettingsSection
            {
                Id = "theme.general",
                PluginId = "_builtin",
                Category = "Theme",
                DisplayName = "General",
                Description = "Theme appearance settings",
                Order = 1,
                Version = 1,
                IconKey = "Palette"
            };

            section.Settings.Add(new SettingDefinition
            {
                Key = "theme.mode",
                DisplayName = "Theme Mode",
                ValueType = SettingValueType.String,
                CurrentValue = Settings.Default.SelectedTheme ?? "Dark",
                UiOrder = 1
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "accent.color",
                DisplayName = "Accent Color",
                ValueType = SettingValueType.Color,
                CurrentValue = Settings.Default.AccentColor ?? "#3A86FF",
                UiOrder = 2
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "animation.speed",
                DisplayName = "Animation Speed",
                ValueType = SettingValueType.Enum,
                EnumValues = new List<string> { "Off", "Reduced", "Full" },
                CurrentValue = Settings.Default["AnimationSpeed"] as string ?? "Full",
                UiOrder = 3
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "ui.density",
                DisplayName = "UI Density",
                ValueType = SettingValueType.Enum,
                EnumValues = new List<string> { "Compact", "Normal", "Spacious" },
                CurrentValue = Settings.Default["UiDensity"] as string ?? "Normal",
                UiOrder = 4
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "background.style",
                DisplayName = "Background Style",
                ValueType = SettingValueType.Enum,
                EnumValues = new List<string> { "Solid", "Gradient" },
                CurrentValue = Settings.Default["BackgroundStyle"] as string ?? "Gradient",
                UiOrder = 5
            });

            _registry.RegisterSection("_builtin", section);
            foreach (var setting in section.Settings)
                _registry.RegisterSetting(section.Id, setting);

            return Task.CompletedTask;
        }

        private Task MigrateAccessibilitySettingsAsync()
        {
            var section = new SettingsSection
            {
                Id = "accessibility.general",
                PluginId = "_builtin",
                Category = "Accessibility",
                DisplayName = "General",
                Description = "Accessibility preferences",
                Order = 1,
                Version = 1,
                IconKey = "Accessibility"
            };

            bool hcEnabled = false;
            if (Settings.Default["HighContrastEnabled"] is bool hc)
                hcEnabled = hc;

            bool fiEnabled = true;
            if (Settings.Default["FocusIndicatorEnabled"] is bool fi)
                fiEnabled = fi;

            section.Settings.Add(new SettingDefinition
            {
                Key = "high.contrast",
                DisplayName = "High Contrast Mode",
                ValueType = SettingValueType.Boolean,
                CurrentValue = hcEnabled,
                UiOrder = 1
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "focus.indicator",
                DisplayName = "Focus Indicator",
                ValueType = SettingValueType.Boolean,
                CurrentValue = fiEnabled,
                UiOrder = 2
            });

            _registry.RegisterSection("_builtin", section);
            foreach (var setting in section.Settings)
                _registry.RegisterSetting(section.Id, setting);

            return Task.CompletedTask;
        }

        private Task MigratePerformanceSettingsAsync()
        {
            var section = new SettingsSection
            {
                Id = "performance.general",
                PluginId = "_builtin",
                Category = "Performance",
                DisplayName = "General",
                Description = "Performance and rendering settings",
                Order = 1,
                Version = 1,
                IconKey = "Speed"
            };

            var renderMode = Settings.Default["RenderMode"] as string ?? "Auto";
            bool safeMode = false;
            if (Settings.Default["SafeModeEnabled"] is bool sm)
                safeMode = sm;

            section.Settings.Add(new SettingDefinition
            {
                Key = "render.mode",
                DisplayName = "Render Mode",
                ValueType = SettingValueType.Enum,
                EnumValues = new List<string> { "Auto", "Safe", "Full" },
                CurrentValue = renderMode,
                UiOrder = 1
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "safe.mode",
                DisplayName = "Safe Mode",
                ValueType = SettingValueType.Boolean,
                CurrentValue = safeMode,
                UiOrder = 2
            });

            _registry.RegisterSection("_builtin", section);
            foreach (var setting in section.Settings)
                _registry.RegisterSetting(section.Id, setting);

            return Task.CompletedTask;
        }

        private Task MigrateDiagnosticsSettingsAsync()
        {
            var section = new SettingsSection
            {
                Id = "diagnostics.general",
                PluginId = "_builtin",
                Category = "Diagnostics",
                DisplayName = "General",
                Description = "Diagnostics and logging settings",
                Order = 1,
                Version = 1,
                IconKey = "Bug"
            };

            section.Settings.Add(new SettingDefinition
            {
                Key = "logging.enabled",
                DisplayName = "Enable Logging",
                ValueType = SettingValueType.Boolean,
                CurrentValue = true,
                UiOrder = 1
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "log.level",
                DisplayName = "Log Level",
                ValueType = SettingValueType.Enum,
                EnumValues = new List<string> { "Debug", "Info", "Warning", "Error" },
                CurrentValue = "Info",
                UiOrder = 2
            });

            _registry.RegisterSection("_builtin", section);
            foreach (var setting in section.Settings)
                _registry.RegisterSetting(section.Id, setting);

            return Task.CompletedTask;
        }

        private Task MigrateExcelSettingsAsync()
        {
            var section = new SettingsSection
            {
                Id = "excel.general",
                PluginId = "_builtin",
                Category = "Excel",
                DisplayName = "General",
                Description = "Excel VSTO integration settings",
                Order = 1,
                Version = 1,
                IconKey = "Excel"
            };

            section.Settings.Add(new SettingDefinition
            {
                Key = "auto.save",
                DisplayName = "Auto-Save on Exit",
                ValueType = SettingValueType.Boolean,
                CurrentValue = true,
                UiOrder = 1
            });

            section.Settings.Add(new SettingDefinition
            {
                Key = "show.toolbar",
                DisplayName = "Show Excel Toolbar",
                ValueType = SettingValueType.Boolean,
                CurrentValue = true,
                UiOrder = 2
            });

            _registry.RegisterSection("_builtin", section);
            foreach (var setting in section.Settings)
                _registry.RegisterSetting(section.Id, setting);

            return Task.CompletedTask;
        }
    }
}
