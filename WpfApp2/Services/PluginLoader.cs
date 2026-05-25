using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class PluginLoader : IPluginLoader
    {
        private readonly Contracts.IModuleRegistry _registry;
        private readonly IServiceContainer _container;
        private readonly Dictionary<string, ModuleManifest> _manifestCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Contracts.IModule> _loadedModules = new(StringComparer.OrdinalIgnoreCase);
        private string? _modulesDirectory;

        public PluginLoader(Contracts.IModuleRegistry registry, IServiceContainer container)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public string ModulesDirectory
        {
            get
            {
                if (_modulesDirectory is null)
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    _modulesDirectory = Path.Combine(baseDir, "Modules");
                }
                return _modulesDirectory;
            }
            set => _modulesDirectory = value;
        }

        public IReadOnlyList<ModuleManifest> DiscoverModules()
        {
            var results = new List<ModuleManifest>();
            var modulesDir = ModulesDirectory;

            if (!Directory.Exists(modulesDir))
                return results;

            foreach (var subDir in Directory.GetDirectories(modulesDir))
            {
                var manifestPath = Path.Combine(subDir, "module.json");
                if (!File.Exists(manifestPath))
                    continue;

                try
                {
                    var json = File.ReadAllText(manifestPath);
                    var manifest = JsonConvert.DeserializeObject<ModuleManifest>(json);
                    if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
                        continue;

                    _manifestCache[manifest.Id] = manifest;
                    results.Add(manifest);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse module manifest at '{manifestPath}': {ex.Message}");
                }
            }

            return results;
        }

        public Contracts.IModule LoadModule(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentNullException(nameof(moduleId));

            if (_loadedModules.TryGetValue(moduleId, out var existing))
                return existing;

            if (!_manifestCache.TryGetValue(moduleId, out var manifest))
                throw new InvalidOperationException($"Module '{moduleId}' was not discovered. Call DiscoverModules() first.");

            var subDir = FindModuleDirectory(moduleId);
            if (subDir is null)
                throw new InvalidOperationException($"Module directory for '{moduleId}' not found in '{ModulesDirectory}'.");

            var assemblyFiles = Directory.GetFiles(subDir, "*.dll");
            if (assemblyFiles.Length == 0)
                throw new InvalidOperationException($"No assemblies found in module directory '{subDir}'.");

            var mainAssemblyPath = assemblyFiles.FirstOrDefault(f =>
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(f);
                    return name.Name is not null && name.Name.EndsWith(moduleId.Split('.').Last(), StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }) ?? assemblyFiles[0];

            var assemblyBytes = File.ReadAllBytes(mainAssemblyPath);

            ValidateIntegrity(moduleId, assemblyBytes, manifest);

            Contracts.IModule module;
            try
            {
                var assembly = Assembly.LoadFile(mainAssemblyPath);
                var moduleType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(Contracts.IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                if (moduleType is null)
                    throw new InvalidOperationException($"No type implementing IModule found in assembly '{mainAssemblyPath}'.");

                module = (Contracts.IModule)Activator.CreateInstance(moduleType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load module '{moduleId}' from '{mainAssemblyPath}': {ex.Message}", ex);
            }

            _loadedModules[moduleId] = module;
            return module;
        }

        private void ValidateIntegrity(string moduleId, byte[] assemblyBytes, ModuleManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Hash))
                throw new InvalidOperationException(
                    $"Assembly integrity check skipped for module '{moduleId}': manifest hash is empty or missing. " +
                    $"Set manifest.Hash to the SHA256 of the assembly file.");

            using var sha256 = SHA256.Create();
            var computedHash = sha256.ComputeHash(assemblyBytes);
            var computed = BitConverter.ToString(computedHash).Replace("-", "").ToUpperInvariant();
            var expected = manifest.Hash.Trim().ToUpperInvariant();

            if (computed != expected)
            {
                throw new InvalidOperationException(
                    $"Assembly integrity check failed for module '{moduleId}'. " +
                    $"Expected hash: {expected}, Computed hash: {computed}.");
            }
        }

        private string? FindModuleDirectory(string moduleId)
        {
            var modulesDir = ModulesDirectory;
            if (!Directory.Exists(modulesDir))
                return null;

            foreach (var subDir in Directory.GetDirectories(modulesDir))
            {
                var manifestPath = Path.Combine(subDir, "module.json");
                if (!File.Exists(manifestPath))
                    continue;

                try
                {
                    var json = File.ReadAllText(manifestPath);
                    var manifest = JsonConvert.DeserializeObject<ModuleManifest>(json);
                    if (manifest?.Id?.Equals(moduleId, StringComparison.OrdinalIgnoreCase) == true)
                        return subDir;
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
