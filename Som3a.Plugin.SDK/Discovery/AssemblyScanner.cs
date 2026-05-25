using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Som3a.Plugin.SDK.Discovery
{
    public class AssemblyScanner
    {
        private readonly string _pluginsDirectory;

        public AssemblyScanner(string pluginsDirectory)
        {
            _pluginsDirectory = pluginsDirectory;
        }

        public List<Assembly> ScanAssemblies()
        {
            var assemblies = new List<Assembly>();

            if (!Directory.Exists(_pluginsDirectory))
                return assemblies;

            var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    var context = new AssemblyLoadContext(dllPath, isCollectible: true);
                    var assembly = context.LoadFromAssemblyPath(dllPath);
                    assemblies.Add(assembly);
                }
                catch
                {
                }
            }

            return assemblies;
        }
    }
}
