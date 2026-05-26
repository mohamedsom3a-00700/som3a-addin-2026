using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Som3a.Contracts;

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

        public List<Type> FindTypesWithNavigationItemAttribute(List<Assembly> assemblies)
        {
            var results = new List<Type>();

            foreach (var assembly in assemblies)
            {
                List<Type>? assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes().ToList();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    assemblyTypes = ex.Types?.Where(t => t != null).ToList()!;
                }

                if (assemblyTypes != null)
                {
                    results.AddRange(assemblyTypes.Where(t =>
                    {
                        try { return t?.GetCustomAttribute<NavigationItemAttribute>() != null; }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"AssemblyScanner: Error checking type '{t?.FullName ?? "<null>"}' in assembly '{t?.Assembly?.FullName ?? "<unknown>"}': {ex.Message}");
                            return false;
                        }
                    }));
                }
            }

            return results;
        }
    }
}
