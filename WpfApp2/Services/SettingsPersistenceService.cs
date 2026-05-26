using Newtonsoft.Json;
using Som3a_WPF_UI.Models;
using Som3a_WPF_UI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

    public sealed class PluginSettingsDocument
    {
        public int Version { get; set; } = 1;
        public string? PluginVersion { get; set; }
        public DateTime LastModified { get; set; }
        public List<string> SectionKeys { get; set; } = new();
        public Dictionary<string, object?> Values { get; set; } = new();
    }

    public sealed class SnapshotBundle
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime ExportedAt { get; set; }
        public string AppVersion { get; set; } = string.Empty;
        public Dictionary<string, PluginSettingsDocument> Plugins { get; set; } = new();
    }

    public sealed class SettingsPersistenceService
    {
        private readonly JsonSerializerSettings _jsonSettings;

        private static readonly string PluginsBasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "Plugins");

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
            if (string.IsNullOrEmpty(settings.SelectedTheme) || !validThemes.Contains(settings.SelectedTheme))
            {
                warnings.Add(string.IsNullOrEmpty(settings.SelectedTheme)
                    ? "Theme not specified. Defaulting to 'Dark'."
                    : $"Unknown theme '{settings.SelectedTheme}'. Defaulting to 'Dark'.");
                settings.SelectedTheme = "Dark";
            }

            var validSpeeds = new[] { "Off", "Reduced", "Full" };
            if (string.IsNullOrEmpty(settings.AnimationSpeed) || !validSpeeds.Contains(settings.AnimationSpeed))
            {
                warnings.Add(string.IsNullOrEmpty(settings.AnimationSpeed)
                    ? "Animation speed not specified. Defaulting to 'Full'."
                    : $"Unknown animation speed '{settings.AnimationSpeed}'. Defaulting to 'Full'.");
                settings.AnimationSpeed = "Full";
            }

            var validDensities = new[] { "Compact", "Normal", "Spacious" };
            if (string.IsNullOrEmpty(settings.UiDensity) || !validDensities.Contains(settings.UiDensity))
            {
                warnings.Add(string.IsNullOrEmpty(settings.UiDensity)
                    ? "UI density not specified. Defaulting to 'Normal'."
                    : $"Unknown UI density '{settings.UiDensity}'. Defaulting to 'Normal'.");
                settings.UiDensity = "Normal";
            }

            var validBgStyles = new[] { "Solid", "Gradient" };
            if (string.IsNullOrEmpty(settings.BackgroundStyle) || !validBgStyles.Contains(settings.BackgroundStyle))
            {
                warnings.Add(string.IsNullOrEmpty(settings.BackgroundStyle)
                    ? "Background style not specified. Defaulting to 'Gradient'."
                    : $"Unknown background style '{settings.BackgroundStyle}'. Defaulting to 'Gradient'.");
                settings.BackgroundStyle = "Gradient";
            }

            var validRenderModes = new[] { "Auto", "Safe", "Full" };
            if (string.IsNullOrEmpty(settings.RenderMode) || !validRenderModes.Contains(settings.RenderMode))
            {
                warnings.Add(string.IsNullOrEmpty(settings.RenderMode)
                    ? "Render mode not specified. Defaulting to 'Auto'."
                    : $"Unknown render mode '{settings.RenderMode}'. Defaulting to 'Auto'.");
                settings.RenderMode = "Auto";
            }
        }

        public async Task<PluginSettingsDocument?> LoadPluginSettingsAsync(string pluginId)
        {
            var filePath = GetPluginSettingsPath(pluginId);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var json = await Task.Run(() => File.ReadAllText(filePath));
                return JsonConvert.DeserializeObject<PluginSettingsDocument>(json, _jsonSettings);
            }
            catch
            {
                return null;
            }
        }

        public async Task SavePluginSettingsAsync(string pluginId, PluginSettingsDocument document)
        {
            var filePath = GetPluginSettingsPath(pluginId);
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            document.LastModified = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(document, _jsonSettings);
            var tempPath = filePath + ".tmp";
            await Task.Run(() =>
            {
                File.WriteAllText(tempPath, json);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.Move(tempPath, filePath);
            });
        }

        public async Task<byte[]?> LoadEncryptedValueAsync(string pluginId, string key)
        {
            var secretsPath = GetPluginSecretsPath(pluginId);
            if (!File.Exists(secretsPath))
                return null;

            try
            {
                var json = await Task.Run(() => File.ReadAllText(secretsPath));
                var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (secrets != null && secrets.TryGetValue(key, out var encryptedBase64))
                {
                    var encryptedBytes = Convert.FromBase64String(encryptedBase64);
                    return ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                }
            }
            catch
            {
            }

            return null;
        }

        public async Task SaveEncryptedValueAsync(string pluginId, string key, byte[] plaintextData)
        {
            var secretsPath = GetPluginSecretsPath(pluginId);
            var dir = Path.GetDirectoryName(secretsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Dictionary<string, string> secrets;
            if (File.Exists(secretsPath))
            {
                try
                {
                    var existingJson = await Task.Run(() => File.ReadAllText(secretsPath));
                    secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(existingJson) ?? new Dictionary<string, string>();
                }
                catch
                {
                    secrets = new Dictionary<string, string>();
                }
            }
            else
            {
                secrets = new Dictionary<string, string>();
            }

            var encryptedBytes = ProtectedData.Protect(plaintextData, null, DataProtectionScope.CurrentUser);
            secrets[key] = Convert.ToBase64String(encryptedBytes);

            var json = JsonConvert.SerializeObject(secrets, _jsonSettings);
            await Task.Run(() => File.WriteAllText(secretsPath, json));
        }

        public async Task ExportSnapshotAsync(string exportPath, IReadOnlyList<string>? pluginIds = null)
        {
            var bundle = new SnapshotBundle
            {
                Version = "1.0.0",
                ExportedAt = DateTime.UtcNow,
                AppVersion = typeof(SettingsPersistenceService).Assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Plugins = new Dictionary<string, PluginSettingsDocument>()
            };

            var pluginsDir = PluginsBasePath;
            if (!Directory.Exists(pluginsDir))
            {
                var json = JsonConvert.SerializeObject(bundle, _jsonSettings);
                var exportDir = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);
                await Task.Run(() => File.WriteAllText(exportPath, json));
                return;
            }

            var pluginDirs = pluginIds != null
                ? pluginIds.Select(id => Path.Combine(pluginsDir, id)).Where(Directory.Exists).ToList()
                : Directory.GetDirectories(pluginsDir).ToList();

            foreach (var dir in pluginDirs)
            {
                var settingsFile = Path.Combine(dir, "settings.json");
                if (!File.Exists(settingsFile))
                    continue;

                try
                {
                    var json = await Task.Run(() => File.ReadAllText(settingsFile));
                    var doc = JsonConvert.DeserializeObject<PluginSettingsDocument>(json, _jsonSettings);
                    if (doc != null)
                        bundle.Plugins[Path.GetFileName(dir)] = doc;
                }
                catch
                {
                }
            }

            var exportDirPath = Path.GetDirectoryName(exportPath);
            if (!string.IsNullOrEmpty(exportDirPath) && !Directory.Exists(exportDirPath))
                Directory.CreateDirectory(exportDirPath);

            var bundleJson = JsonConvert.SerializeObject(bundle, _jsonSettings);
            await Task.Run(() => File.WriteAllText(exportPath, bundleJson));
        }

        public async Task<SnapshotBundle?> ImportSnapshotAsync(string importPath, IReadOnlyList<string>? pluginIds = null)
        {
            if (!File.Exists(importPath))
                throw new FileNotFoundException("Import file not found.", importPath);

            string json;
            try
            {
                json = await Task.Run(() => File.ReadAllText(importPath));
            }
            catch (Exception ex)
            {
                throw new SettingsImportException("Failed to read import file.", ex);
            }

            SnapshotBundle? bundle;
            try
            {
                bundle = JsonConvert.DeserializeObject<SnapshotBundle>(json, _jsonSettings);
            }
            catch (JsonReaderException ex)
            {
                throw new SettingsImportException("Invalid JSON format in import file.", ex);
            }

            if (bundle == null)
                throw new SettingsImportException("Invalid import file format.");

            var pluginsToImport = pluginIds != null
                ? bundle.Plugins.Where(p => pluginIds.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value)
                : bundle.Plugins;

            foreach (var kvp in pluginsToImport)
            {
                var pluginId = kvp.Key;
                var doc = kvp.Value;
                if (doc == null) continue;

                var filePath = GetPluginSettingsPath(pluginId);
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var docJson = JsonConvert.SerializeObject(doc, _jsonSettings);
                await Task.Run(() => File.WriteAllText(filePath, docJson));
            }

            return bundle;
        }

        private static string GetPluginSettingsPath(string pluginId)
        {
            var sanitized = SanitizePluginId(pluginId);
            var path = Path.Combine(PluginsBasePath, sanitized, "settings.json");
            ValidatePluginPath(path);
            return path;
        }

        private static string GetPluginSecretsPath(string pluginId)
        {
            var sanitized = SanitizePluginId(pluginId);
            var path = Path.Combine(PluginsBasePath, sanitized, "secrets.json");
            ValidatePluginPath(path);
            return path;
        }

        private static string SanitizePluginId(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID must not be null or empty.", nameof(pluginId));

            var invalid = Path.GetInvalidFileNameChars();
            if (pluginId.IndexOfAny(invalid) >= 0)
                throw new ArgumentException($"Plugin ID contains invalid characters: '{pluginId}'", nameof(pluginId));

            return pluginId;
        }

        private static void ValidatePluginPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var baseFull = Path.GetFullPath(PluginsBasePath);
            if (!fullPath.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Path traversal detected for plugin path: '{path}'");
        }
    }
}
