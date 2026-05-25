using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;

namespace Som3a_WPF_UI.Services
{
    public interface IValidationEngine
    {
        IReadOnlyList<ValidationResult> RunValidation();
        event EventHandler<IReadOnlyList<ValidationResult>> ValidationCompleted;
    }

    public sealed class ValidationEngine : IValidationEngine
    {
        private readonly ILoggingService _loggingService;
        private int _scanCounter;

        public event EventHandler<IReadOnlyList<ValidationResult>> ValidationCompleted;

        public ValidationEngine(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public IReadOnlyList<ValidationResult> RunValidation()
        {
            var results = new List<ValidationResult>();
            _scanCounter++;

            try
            {
                if (Application.Current?.Resources?.MergedDictionaries == null)
                {
                    _loggingService.Log("WARN", "Validation", "No resource dictionaries available for validation", "ValidationEngine");
                    return results;
                }

                var seenTypes = new HashSet<string>();

                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict == null) continue;

                    var dictName = dict.Source?.ToString() ?? "InlineDictionary";

                    results.AddRange(ScanForMissingTokens(dict, dictName));
                    results.AddRange(ScanForInlineColors(dict, dictName));
                    results.AddRange(ScanForDuplicateStyles(dict, dictName, seenTypes));
                    results.AddRange(ScanForInvalidResources(dict, dictName));
                }

                var appDirectory = Path.GetDirectoryName(Application.Current?.GetType().Assembly.Location);
                if (appDirectory != null)
                {
                    var projectRoot = Directory.GetParent(appDirectory)?.Parent?.Parent?.FullName;
                    if (projectRoot != null)
                        results.AddRange(ScanAllXamlFiles(projectRoot));
                }

                var readOnlyResults = results.AsReadOnly();
                _loggingService.Log("INFO", "Validation", $"Scan completed: {results.Count} issues found", "ValidationEngine");
                ValidationCompleted?.Invoke(this, readOnlyResults);
            }
            catch (Exception ex)
            {
                _loggingService.Log("ERROR", "Validation", $"Validation scan failed: {ex.Message}", "ValidationEngine", ex.ToString());
            }

            return results;
        }

        private List<ValidationResult> ScanForMissingTokens(ResourceDictionary dict, string dictName)
        {
            var results = new List<ValidationResult>();

            try
            {
                var requiredTokens = new[]
                {
                    "Brush.Background.Primary", "Brush.Text.Primary", "Brush.Accent.Primary",
                    "AccentColorBrush", "AccentColorValue", "Shadow.Card", "Shadow.Popup", "Shadow.Window"
                };

                foreach (var token in requiredTokens)
                {
                    if (!dict.Contains(token) && Application.Current.Resources[token] == null)
                    {
                        results.Add(new ValidationResult
                        {
                            Id = $"VR-MT-{_scanCounter:D3}-{results.Count + 1:D3}",
                            Severity = "error",
                            Category = "missing-token",
                            DictionaryName = dictName,
                            Location = token,
                            Description = $"Required token '{token}' is missing from all loaded dictionaries",
                            SuggestedFix = $"Add <SolidColorBrush x:Key=\"{token}\" .../> to Colors.xaml",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning for missing tokens in {dictName}: {ex.Message}", "ValidationEngine");
            }

            return results;
        }

        private List<ValidationResult> ScanForInlineColors(ResourceDictionary dict, string dictName)
        {
            var results = new List<ValidationResult>();

            try
            {
                foreach (var key in dict.Keys)
                {
                    var value = dict[key];
                    if (value is System.Windows.Media.Color)
                    {
                        results.Add(new ValidationResult
                        {
                            Id = $"VR-IC-{_scanCounter:D3}-{results.Count + 1:D3}",
                            Severity = "error",
                            Category = "inline-color",
                            DictionaryName = dictName,
                            Location = key?.ToString() ?? "Unknown",
                            Description = $"Inline Color resource found at key '{key}'",
                            SuggestedFix = "Replace with a DynamicResource reference to a semantic token",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning for inline colors in {dictName}: {ex.Message}", "ValidationEngine");
            }

            return results;
        }

        private List<ValidationResult> ScanForDuplicateStyles(ResourceDictionary dict, string dictName, HashSet<string> seenTypes)
        {
            var results = new List<ValidationResult>();

            try
            {
                foreach (var key in dict.Keys)
                {
                    if (dict[key] is Style style && style.TargetType != null)
                    {
                        var typeName = style.TargetType.FullName;
                        if (!seenTypes.Add(typeName))
                        {
                            results.Add(new ValidationResult
                            {
                                Id = $"VR-DS-{_scanCounter:D3}-{results.Count + 1:D3}",
                                Severity = "warning",
                                Category = "duplicate-style",
                                DictionaryName = dictName,
                                Location = typeName,
                                Description = $"Duplicate Style for TargetType '{typeName}'",
                                SuggestedFix = "Remove duplicate style definition; use BasedOn for variations",
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning for duplicate styles in {dictName}: {ex.Message}", "ValidationEngine");
            }

            return results;
        }

        private readonly Dictionary<string, string> _resourceRegistry = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Style, string> _styleInstanceMap = new Dictionary<Style, string>();

        private void BuildResourceRegistry()
        {
            _resourceRegistry.Clear();
            _styleInstanceMap.Clear();

            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict?.Source == null) continue;
                var dictPath = dict.Source.OriginalString;
                foreach (var key in dict.Keys)
                {
                    if (key is string strKey)
                    {
                        _resourceRegistry[strKey] = dictPath;
                        if (dict[strKey] is Style s)
                            _styleInstanceMap[s] = strKey;
                    }
                }
            }
        }

        private void ScanXamlFileForInlineIssues(string filePath, List<ValidationResult> results)
        {
            try
            {
                if (!File.Exists(filePath)) return;
                var xaml = File.ReadAllText(filePath);
                var doc = XDocument.Parse(xaml);
                var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

                foreach (var element in doc.Descendants())
                {
                    var lineInfo = (IXmlLineInfo)element;

                    foreach (var attr in element.Attributes())
                    {
                        if (attr.Value.StartsWith("#") && attr.Value.Length >= 7 && attr.Value.Length <= 9)
                        {
                            if (filePath.Contains("\\Base\\")) continue;

                            var isTokenDefinition = attr.Name.LocalName == "Color"
                                && (element.Name.LocalName == "SolidColorBrush" || element.Name.LocalName == "Color");
                            if (isTokenDefinition) continue;

                            results.Add(new ValidationResult
                            {
                                Id = $"VR-FS-{_scanCounter:D3}-{results.Count + 1:D3}",
                                Severity = "warning",
                                Category = "file-scan-inline-color",
                                DictionaryName = filePath,
                                Location = $"Line {lineInfo.LineNumber}, Attr '{attr.Name.LocalName}'",
                                Description = $"Hardcoded color '{attr.Value}' found in {Path.GetFileName(filePath)}",
                                SuggestedFix = "Replace with a DynamicResource reference to a semantic token",
                                Timestamp = DateTime.UtcNow
                            });
                        }

                        if ((attr.Name.LocalName == "Margin" || attr.Name.LocalName == "Padding")
                            && attr.Value.Any(char.IsDigit)
                            && !attr.Value.Contains("{DynamicResource"))
                        {
                            results.Add(new ValidationResult
                            {
                                Id = $"VR-FS-{_scanCounter:D3}-{results.Count + 1:D3}",
                                Severity = "info",
                                Category = "file-scan-raw-dimension",
                                DictionaryName = filePath,
                                Location = $"Line {lineInfo.LineNumber}, Attr '{attr.Name.LocalName}'",
                                Description = $"Raw {attr.Name.LocalName}='{attr.Value}' in {Path.GetFileName(filePath)}",
                                SuggestedFix = "Consider using a Spacing/Padding token from Theme/Base/",
                                Timestamp = DateTime.UtcNow
                            });
                        }

                        if (attr.Name.LocalName == "CornerRadius"
                            && attr.Value.Any(char.IsDigit)
                            && !attr.Value.Contains("{DynamicResource"))
                        {
                            results.Add(new ValidationResult
                            {
                                Id = $"VR-FS-{_scanCounter:D3}-{results.Count + 1:D3}",
                                Severity = "info",
                                Category = "file-scan-raw-radius",
                                DictionaryName = filePath,
                                Location = $"Line {lineInfo.LineNumber}, Attr '{attr.Name.LocalName}'",
                                Description = $"Raw CornerRadius='{attr.Value}' in {Path.GetFileName(filePath)}",
                                SuggestedFix = "Consider using a Radius token from Theme/Base/Radius.xaml",
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning file {filePath}: {ex.Message}", "ValidationEngine");
            }
        }

        private void ValidateStyleBasedOn(Style style, string dictName, string styleKey, List<ValidationResult> results)
        {
            if (style.BasedOn == null) return;

            string basedOnKey;
            if (_styleInstanceMap.TryGetValue(style.BasedOn, out var mappedKey))
                basedOnKey = mappedKey;
            else
                basedOnKey = style.BasedOn.TargetType?.Name ?? "Unknown";

            if (!_resourceRegistry.ContainsKey(basedOnKey))
            {
                results.Add(new ValidationResult
                {
                    Id = $"VR-BO-{_scanCounter:D3}-{results.Count + 1:D3}",
                    Severity = "warning",
                    Category = "basedon-not-found",
                    DictionaryName = dictName,
                    Location = $"Style Key='{styleKey}', BasedOn='{basedOnKey}'",
                    Description = $"Style '{styleKey}' references BasedOn='{basedOnKey}' which was not found in any loaded dictionary",
                    SuggestedFix = $"Ensure the based-on style is defined in a loaded dictionary before this style",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        private List<ValidationResult> ScanForInvalidResources(ResourceDictionary dict, string dictName)
        {
            BuildResourceRegistry();
            var results = new List<ValidationResult>();

            try
            {
                foreach (var key in dict.Keys)
                {
                    if (dict[key] is Style style)
                    {
                        var styleKey = key?.ToString() ?? "Unnamed";
                        ValidateStyleBasedOn(style, dictName, styleKey, results);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning for invalid resources in {dictName}: {ex.Message}", "ValidationEngine");
            }

            return results;
        }

        public IReadOnlyList<ValidationResult> ScanAllXamlFiles(string rootDirectory)
        {
            var results = new List<ValidationResult>();
            if (!Directory.Exists(rootDirectory)) return results;

            var xamlFiles = Directory.GetFiles(rootDirectory, "*.xaml", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
                .ToList();

            _loggingService.Log("INFO", "Validation", $"Scanning {xamlFiles.Count} XAML files for inline issues", "ValidationEngine");

            foreach (var file in xamlFiles)
            {
                ScanXamlFileForInlineIssues(file, results);
            }

            _loggingService.Log("INFO", "Validation", $"File scan complete: {results.Count} issues found", "ValidationEngine");
            return results.AsReadOnly();
        }
    }
}
