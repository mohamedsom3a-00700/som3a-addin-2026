using Newtonsoft.Json;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public sealed class SettingsPersistenceException : Exception
    {
        public SettingsPersistenceException(string message) : base(message) { }
        public SettingsPersistenceException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class SettingsImportException : Exception
    {
        public SettingsImportException(string message) : base(message) { }
        public SettingsImportException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class SettingsPersistenceService
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public SettingsPersistenceService()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public UserSettings LoadSettings()
        {
            var settings = new UserSettings();
            try
            {
                settings.SelectedTheme = Settings.Default.SelectedTheme;
                settings.AccentColor = Settings.Default.AccentColor;

                var animationSpeed = Settings.Default["AnimationSpeed"] as string;
                if (!string.IsNullOrEmpty(animationSpeed))
                    settings.AnimationSpeed = animationSpeed;

                var uiDensity = Settings.Default["UiDensity"] as string;
                if (!string.IsNullOrEmpty(uiDensity))
                    settings.UiDensity = uiDensity;

                var backgroundStyle = Settings.Default["BackgroundStyle"] as string;
                if (!string.IsNullOrEmpty(backgroundStyle))
                    settings.BackgroundStyle = backgroundStyle;

                if (Settings.Default["HighContrastEnabled"] is bool hc)
                    settings.HighContrastEnabled = hc;

                if (Settings.Default["FocusIndicatorEnabled"] is bool fi)
                    settings.FocusIndicatorEnabled = fi;

                var renderMode = Settings.Default["RenderMode"] as string;
                if (!string.IsNullOrEmpty(renderMode))
                    settings.RenderMode = renderMode;

                if (Settings.Default["SafeModeEnabled"] is bool sm)
                    settings.SafeModeEnabled = sm;
            }
            catch
            {
                // Return defaults if settings not yet configured
            }
            return settings;
        }

        public void SaveSettings(UserSettings settings)
        {
            try
            {
                Settings.Default.SelectedTheme = settings.SelectedTheme;
                Settings.Default.AccentColor = settings.AccentColor;
                Settings.Default["AnimationSpeed"] = settings.AnimationSpeed;
                Settings.Default["UiDensity"] = settings.UiDensity;
                Settings.Default["BackgroundStyle"] = settings.BackgroundStyle;
                Settings.Default["HighContrastEnabled"] = settings.HighContrastEnabled;
                Settings.Default["FocusIndicatorEnabled"] = settings.FocusIndicatorEnabled;
                Settings.Default["RenderMode"] = settings.RenderMode;
                Settings.Default["SafeModeEnabled"] = settings.SafeModeEnabled;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                throw new SettingsPersistenceException("Failed to save settings.", ex);
            }
        }

        public void ExportSettings(UserSettings settings, string filePath)
        {
            var export = new SettingsExport
            {
                Version = "1.0.0",
                ExportedAt = DateTime.UtcNow,
                Settings = settings,
                AppVersion = typeof(SettingsPersistenceService).Assembly.GetName().Version?.ToString() ?? "0.0.0.0"
            };

            var json = JsonConvert.SerializeObject(export, _jsonSettings);
            File.WriteAllText(filePath, json);
        }

        public ImportResult ImportSettings(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Settings file not found.", filePath);

            string json;
            try
            {
                json = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new SettingsImportException("Invalid file format.", ex);
            }

            SettingsExport? export;
            try
            {
                export = JsonConvert.DeserializeObject<SettingsExport>(json, _jsonSettings);
            }
            catch (JsonReaderException ex)
            {
                throw new SettingsImportException("Invalid file format.", ex);
            }

            if (export == null)
                throw new SettingsImportException("Invalid file format.");

            if (string.IsNullOrEmpty(export.Version))
                throw new SettingsImportException("Missing required field: version.");

            if (export.Settings == null)
                throw new SettingsImportException("Missing required field: settings.");

            var result = new ImportResult
            {
                Settings = export.Settings,
                Warnings = new List<string>()
            };

            ValidateSettings(result.Settings, result.Warnings);

            return result;
        }

        private static void ValidateSettings(UserSettings settings, List<string> warnings)
        {
            var validThemes = new[] { "Dark", "Light", "Custom" };
            if (!string.IsNullOrEmpty(settings.SelectedTheme) &&
                !validThemes.Contains(settings.SelectedTheme))
            {
                warnings.Add($"Unknown theme '{settings.SelectedTheme}'. Defaulting to 'Dark'.");
                settings.SelectedTheme = "Dark";
            }

            var validSpeeds = new[] { "Off", "Reduced", "Full" };
            if (!string.IsNullOrEmpty(settings.AnimationSpeed) &&
                !validSpeeds.Contains(settings.AnimationSpeed))
            {
                warnings.Add($"Unknown animation speed '{settings.AnimationSpeed}'. Defaulting to 'Full'.");
                settings.AnimationSpeed = "Full";
            }

            var validDensities = new[] { "Compact", "Normal", "Spacious" };
            if (!string.IsNullOrEmpty(settings.UiDensity) &&
                !validDensities.Contains(settings.UiDensity))
            {
                warnings.Add($"Unknown UI density '{settings.UiDensity}'. Defaulting to 'Normal'.");
                settings.UiDensity = "Normal";
            }

            var validBgStyles = new[] { "Solid", "Gradient" };
            if (!string.IsNullOrEmpty(settings.BackgroundStyle) &&
                !validBgStyles.Contains(settings.BackgroundStyle))
            {
                warnings.Add($"Unknown background style '{settings.BackgroundStyle}'. Defaulting to 'Gradient'.");
                settings.BackgroundStyle = "Gradient";
            }

            var validRenderModes = new[] { "Auto", "Safe", "Full" };
            if (!string.IsNullOrEmpty(settings.RenderMode) &&
                !validRenderModes.Contains(settings.RenderMode))
            {
                warnings.Add($"Unknown render mode '{settings.RenderMode}'. Defaulting to 'Auto'.");
                settings.RenderMode = "Auto";
            }
        }
    }
}
