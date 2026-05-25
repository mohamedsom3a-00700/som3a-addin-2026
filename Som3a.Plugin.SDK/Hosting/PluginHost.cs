using System;
using System.Collections.Generic;
using System.Linq;
using Som3a.Contracts;
using Som3a.Plugin.SDK.Discovery;
using Som3a.Plugin.SDK.Validation;

namespace Som3a.Plugin.SDK.Hosting
{
    public class PluginHost
    {
        private readonly PluginDiscoverer _discoverer;
        private readonly PluginValidator _validator;
        private readonly List<PluginSandbox> _sandboxes = new();
        private readonly IPluginContext _pluginContext;

        public IReadOnlyList<PluginSandbox> Sandboxes => _sandboxes.AsReadOnly();

        public PluginHost(PluginDiscoverer discoverer, PluginValidator validator, IPluginContext pluginContext)
        {
            _discoverer = discoverer;
            _validator = validator;
            _pluginContext = pluginContext;
        }

        public List<PluginDescriptor> LoadAll()
        {
            var discovered = _discoverer.Discover();
            var validated = _validator.Validate(discovered);

            foreach (var descriptor in validated)
            {
                var sandbox = new PluginSandbox(descriptor);
                if (sandbox.Load())
                {
                    if (sandbox.Initialize(_pluginContext))
                    {
                        _sandboxes.Add(sandbox);
                    }
                }
            }

            return validated;
        }

        public void ShutdownAll()
        {
            foreach (var sandbox in _sandboxes)
            {
                try { sandbox.Shutdown(); }
                catch { }
            }
            _sandboxes.Clear();
        }

        public PluginDescriptor? GetDescriptor(string pluginId)
        {
            var sb = _sandboxes.FirstOrDefault(s => s.Descriptor.Id == pluginId);
            return sb?.Descriptor;
        }
    }
}
