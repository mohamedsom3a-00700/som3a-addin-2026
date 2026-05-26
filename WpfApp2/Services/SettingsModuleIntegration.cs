using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public interface ISettingsModule
    {
        string ModuleId { get; }
        string ModuleName { get; }
        void RegisterSettings(SettingsRegistry registry);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SettingsSectionAttribute : Attribute
    {
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? IconKey { get; set; }
    }

    public sealed class SettingsModuleIntegration
    {
        private readonly SettingsRegistry _registry;

        public SettingsModuleIntegration(SettingsRegistry registry)
        {
            _registry = registry;
        }

        public void DiscoverAndRegisterModules(Assembly assembly)
        {
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(ISettingsModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var type in moduleTypes)
            {
                var attr = type.GetCustomAttribute<SettingsSectionAttribute>();
                var module = Activator.CreateInstance(type) as ISettingsModule;
                if (module == null) continue;

                try
                {
                    module.RegisterSettings(_registry);

                    if (attr != null)
                    {
                        var sections = _registry.GetSectionsForPlugin(module.ModuleId);
                        foreach (var section in sections)
                        {
                            if (string.IsNullOrEmpty(section.Category))
                                section.Category = attr.Category;
                            if (section.Order == 0)
                                section.Order = attr.Order;
                            if (string.IsNullOrEmpty(section.IconKey))
                                section.IconKey = attr.IconKey;
                        }
                    }

                    _registry.SetPluginState(module.ModuleId, PluginSettingsState.Active);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register settings for module '{module.ModuleId}': {ex.Message}");
                }
            }
        }

        public void RegisterBuiltinSections(IEnumerable<SettingsSection> sections)
        {
            foreach (var section in sections)
            {
                try
                {
                    _registry.RegisterSection("_builtin", section);
                    foreach (var setting in section.Settings)
                        _registry.RegisterSetting(section.Id, setting);
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping duplicate section '{section.Id}': {ex.Message}");
                }
            }
        }

        public void OnPluginUnloaded(string pluginId)
        {
            _registry.SetPluginState(pluginId, PluginSettingsState.Orphaned);
        }

        public void OnPluginReloaded(string pluginId)
        {
            _registry.SetPluginState(pluginId, PluginSettingsState.Active);
        }
    }
}
