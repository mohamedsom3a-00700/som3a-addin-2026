using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Som3a.Contracts;
using Som3a.Plugin.SDK.Attributes;

namespace Som3a.Plugin.SDK.Discovery
{
    public class PluginDescriptor
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Priority { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public string AssemblyPath { get; set; } = string.Empty;
        public PluginStatus Status { get; set; }
        public Type? PluginType { get; set; }
        public AssemblyLoadContext? LoadContext { get; set; }
    }

    public enum PluginStatus
    {
        Discovered,
        Loaded,
        Error,
        Disabled
    }

    public class PluginDiscoverer
    {
        private readonly AssemblyScanner _scanner;

        public PluginDiscoverer(AssemblyScanner scanner)
        {
            _scanner = scanner;
        }

        public List<PluginDescriptor> Discover()
        {
            var descriptors = new List<PluginDescriptor>();
            var assemblies = _scanner.ScanAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetExportedTypes();

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<PluginAttribute>();
                    if (attr == null) continue;

                    if (!typeof(IPlugin).IsAssignableFrom(type)) continue;

                    var descriptor = new PluginDescriptor
                    {
                        Id = attr.Id,
                        Name = attr.Name,
                        Version = attr.Version,
                        Priority = attr.Priority,
                        Dependencies = attr.Dependencies?.ToList() ?? new List<string>(),
                        AssemblyPath = assembly.Location,
                        Status = PluginStatus.Discovered,
                        PluginType = type,
                        LoadContext = AssemblyLoadContext.GetLoadContext(assembly)
                    };

                    descriptors.Add(descriptor);
                }
            }

            return descriptors;
        }
    }
}
