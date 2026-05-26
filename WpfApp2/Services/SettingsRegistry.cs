using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public enum SettingValueType
    {
        String,
        Integer,
        Decimal,
        Boolean,
        Enum,
        Color,
        FilePath,
        Secret
    }

    public enum ValidationSeverity
    {
        Error,
        Warning
    }

    public enum PluginSettingsState
    {
        Registered,
        Active,
        Orphaned
    }

    public sealed class ValidationRule
    {
        public string RuleId { get; set; } = string.Empty;
        public string? Parameters { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    }

    public sealed class SettingDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SettingValueType ValueType { get; set; }
        public object? DefaultValue { get; set; }
        public object? CurrentValue { get; set; }
        public List<string>? EnumValues { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? StepSize { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsReadOnly { get; set; }
        public List<ValidationRule>? ValidationRules { get; set; }
        public int UiOrder { get; set; }
        public string SectionId { get; set; } = string.Empty;
    }

    public sealed class SettingsSection
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public int Version { get; set; } = 1;
        public string? IconKey { get; set; }
        public List<SettingDefinition> Settings { get; set; } = new();
        public List<ValidationRule>? SectionValidationRules { get; set; }
    }

    public sealed class SettingsPersistenceManifest
    {
        public int Version { get; set; } = 1;
        public string? PluginVersion { get; set; }
        public DateTime LastModified { get; set; }
        public List<string> SectionKeys { get; set; } = new();
        public string? MigratedFrom { get; set; }
    }

    public sealed class SettingsRegistry
    {
        private readonly Dictionary<string, List<SettingsSection>> _pluginSections = new();
        private readonly Dictionary<string, SettingsSection> _sectionIndex = new();
        private readonly Dictionary<string, PluginSettingsState> _pluginStates = new();
        private readonly Dictionary<string, object> _defaultValues = new();
        private readonly HashSet<string> _categories = new();

        private static readonly Lazy<SettingsRegistry> _instance = new(() => new SettingsRegistry());
        public static SettingsRegistry Instance => _instance.Value;

        public event EventHandler<SettingsChangedEventArgs>? SettingChanged;

        private SettingsRegistry() { }

        public void RegisterSection(string pluginId, SettingsSection section)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID must be non-empty.", nameof(pluginId));

            if (string.IsNullOrEmpty(section.Id))
                throw new ArgumentException("Section ID must be non-empty.");

            if (string.IsNullOrEmpty(section.Category))
                section.Category = "Other";

            if (section.Order < 0)
                section.Order = 0;

            if (section.Version < 1)
                section.Version = 1;

            if (!_pluginSections.ContainsKey(pluginId))
                _pluginSections[pluginId] = new List<SettingsSection>();

            var existing = _pluginSections[pluginId].FirstOrDefault(s => s.Id == section.Id);
            if (existing != null)
                throw new InvalidOperationException($"Section with ID '{section.Id}' already registered for plugin '{pluginId}'.");

            section.PluginId = pluginId;
            _pluginSections[pluginId].Add(section);
            _sectionIndex[section.Id] = section;
            _categories.Add(section.Category);

            if (!_pluginStates.ContainsKey(pluginId))
                _pluginStates[pluginId] = PluginSettingsState.Active;
        }

        public void RegisterSetting(string sectionId, SettingDefinition setting)
        {
            if (!_sectionIndex.TryGetValue(sectionId, out var section))
                throw new InvalidOperationException($"Section with ID '{sectionId}' not found.");

            if (string.IsNullOrEmpty(setting.Key))
                throw new ArgumentException("Setting key must be non-empty.");

            if (section.Settings.Any(s => s.Key == setting.Key))
                throw new InvalidOperationException($"Setting with key '{setting.Key}' already exists in section '{sectionId}'.");

            if (setting.ValueType == SettingValueType.Secret)
                setting.IsEncrypted = true;

            if (setting.ValueType == SettingValueType.Enum && (setting.EnumValues == null || setting.EnumValues.Count == 0))
                throw new InvalidOperationException($"Enum values must be non-empty for Enum type setting '{setting.Key}'.");

            setting.SectionId = sectionId;
            section.Settings.Add(setting);

            if (setting.DefaultValue != null)
                _defaultValues[$"{sectionId}.{setting.Key}"] = setting.DefaultValue;
        }

        public IReadOnlyList<SettingsSection> GetSectionsByCategory(string category)
        {
            return _pluginSections.Values
                .SelectMany(list => list)
                .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Order)
                .ToList();
        }

        public IReadOnlyList<string> GetAllCategories()
        {
            return _categories.OrderBy(c => c).ToList();
        }

        public SettingDefinition? GetSetting(string sectionId, string key)
        {
            if (!_sectionIndex.TryGetValue(sectionId, out var section))
                return null;

            return section.Settings.FirstOrDefault(s => s.Key == key);
        }

        public object? GetSettingValue(string sectionId, string key)
        {
            var setting = GetSetting(sectionId, key);
            return setting?.CurrentValue ?? setting?.DefaultValue;
        }

        public void UpdateSettingValue(string sectionId, string key, object? value)
        {
            var setting = GetSetting(sectionId, key);
            if (setting == null)
                throw new InvalidOperationException($"Setting '{key}' not found in section '{sectionId}'.");

            var oldValue = setting.CurrentValue;
            setting.CurrentValue = value;

            if (!_sectionIndex.TryGetValue(sectionId, out var section))
                return;

            SettingChanged?.Invoke(this, new SettingsChangedEventArgs
            {
                ModuleId = section.PluginId,
                SectionId = sectionId,
                SettingKey = key,
                OldValue = oldValue,
                NewValue = value,
                ChangedAt = DateTime.UtcNow
            });
        }

        public IReadOnlyList<SettingsSection> GetSectionsForPlugin(string pluginId)
        {
            return _pluginSections.TryGetValue(pluginId, out var sections)
                ? sections.OrderBy(s => s.Order).ToList()
                : Array.Empty<SettingsSection>();
        }

        public void PurgePluginSections(string pluginId)
        {
            if (!_pluginSections.TryGetValue(pluginId, out var sections))
                return;

            foreach (var section in sections)
                _sectionIndex.Remove(section.Id);

            _pluginSections.Remove(pluginId);
            _pluginStates.Remove(pluginId);

            RebuildCategories();
        }

        public PluginSettingsState GetPluginState(string pluginId)
        {
            return _pluginStates.TryGetValue(pluginId, out var state) ? state : PluginSettingsState.Orphaned;
        }

        public void SetPluginState(string pluginId, PluginSettingsState state)
        {
            _pluginStates[pluginId] = state;
        }

        public IReadOnlyList<SettingsSection> GetOrphanedSections()
        {
            return _pluginSections
                .Where(kvp => _pluginStates.TryGetValue(kvp.Key, out var state) && state == PluginSettingsState.Orphaned)
                .SelectMany(kvp => kvp.Value)
                .ToList();
        }

        public bool IsDirty { get; private set; }

        public void MarkClean()
        {
            IsDirty = false;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        private void RebuildCategories()
        {
            _categories.Clear();
            foreach (var section in _sectionIndex.Values)
                _categories.Add(section.Category);
        }
    }

    public sealed class SettingsChangedEventArgs : EventArgs
    {
        public string ModuleId { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
