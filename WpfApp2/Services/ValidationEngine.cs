using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

        private List<ValidationResult> ScanForInvalidResources(ResourceDictionary dict, string dictName)
        {
            var results = new List<ValidationResult>();

            try
            {
                foreach (var key in dict.Keys)
                {
                    if (dict[key] is Style style)
                    {
                        if (style.BasedOn != null)
                        {
                            // TODO: proper XAML/dictionary resolver needed to validate
                            // style.BasedOn at runtime without emitting false warnings
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log("WARN", "Validation", $"Error scanning for invalid resources in {dictName}: {ex.Message}", "ValidationEngine");
            }

            return results;
        }
    }
}
