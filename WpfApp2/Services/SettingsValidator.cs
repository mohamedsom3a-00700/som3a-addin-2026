using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Som3a_WPF_UI.Services
{
    public sealed class SettingValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationError> Warnings { get; set; } = new();
    }

    public sealed class ValidationError
    {
        public string SettingKey { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ValidationSeverity Severity { get; set; }
    }

    public sealed class SettingsValidator
    {
        public SettingValidationResult Validate(SettingDefinition setting, object? value)
        {
            var result = new SettingValidationResult();

            if (setting.ValidationRules == null || setting.ValidationRules.Count == 0)
                return result;

            foreach (var rule in setting.ValidationRules)
            {
                var error = ExecuteRule(rule, setting, value);
                if (error != null)
                {
                    if (error.Severity == ValidationSeverity.Error)
                    {
                        result.Errors.Add(error);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.Warnings.Add(error);
                    }
                }
            }

            return result;
        }

        public SettingValidationResult ValidateAll(IEnumerable<SettingDefinition> settings, Func<SettingDefinition, object?> valueProvider)
        {
            var result = new SettingValidationResult();

            foreach (var setting in settings)
            {
                var settingResult = Validate(setting, valueProvider(setting));
                result.Errors.AddRange(settingResult.Errors);
                result.Warnings.AddRange(settingResult.Warnings);
                if (!settingResult.IsValid)
                    result.IsValid = false;
            }

            return result;
        }

        private static ValidationError? ExecuteRule(ValidationRule rule, SettingDefinition setting, object? value)
        {
            switch (rule.RuleId.ToLowerInvariant())
            {
                case "required":
                    return ValidateRequired(rule, setting, value);

                case "range":
                    return ValidateRange(rule, setting, value);

                case "regex":
                    return ValidateRegex(rule, setting, value);

                case "minlength":
                    return ValidateMinLength(rule, setting, value);

                case "maxlength":
                    return ValidateMaxLength(rule, setting, value);

                case "filepathexists":
                    return ValidateFilePathExists(rule, setting, value);

                default:
                    return null;
            }
        }

        private static ValidationError? ValidateRequired(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return CreateError(setting, rule);
            }
            return null;
        }

        private static ValidationError? ValidateRange(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value == null) return null;

            var parameters = ParseParameters(rule.Parameters);
            double min = ParseDoubleParameter(parameters, "min", double.MinValue);
            double max = ParseDoubleParameter(parameters, "max", double.MaxValue);

            double numericValue;
            if (value is double d)
                numericValue = d;
            else if (value is int i)
                numericValue = i;
            else if (value is decimal dec)
                numericValue = (double)dec;
            else if (value is long l)
                numericValue = l;
            else if (value is float f)
                numericValue = f;
            else if (value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                numericValue = parsed;
            else
                return null;

            if (numericValue < min || numericValue > max)
                return CreateError(setting, rule);

            return null;
        }

        private static ValidationError? ValidateRegex(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value is not string str || string.IsNullOrEmpty(str))
                return null;

            var parameters = ParseParameters(rule.Parameters);
            if (!parameters.TryGetValue("pattern", out var pattern) || string.IsNullOrEmpty(pattern))
                return null;

            if (!Regex.IsMatch(str, pattern, RegexOptions.Compiled))
                return CreateError(setting, rule);

            return null;
        }

        private static ValidationError? ValidateMinLength(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value is not string str) return null;

            var parameters = ParseParameters(rule.Parameters);
            int min = (int)ParseDoubleParameter(parameters, "min", 0);

            if (str.Length < min)
                return CreateError(setting, rule);

            return null;
        }

        private static ValidationError? ValidateMaxLength(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value is not string str) return null;

            var parameters = ParseParameters(rule.Parameters);
            int max = (int)ParseDoubleParameter(parameters, "max", int.MaxValue);

            if (str.Length > max)
                return CreateError(setting, rule);

            return null;
        }

        private static ValidationError? ValidateFilePathExists(ValidationRule rule, SettingDefinition setting, object? value)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return null;

            if (!File.Exists(path) && !Directory.Exists(path))
                return CreateError(setting, rule);

            return null;
        }

        private static ValidationError CreateError(SettingDefinition setting, ValidationRule rule)
        {
            return new ValidationError
            {
                SettingKey = setting.Key,
                SectionId = setting.SectionId,
                Message = rule.ErrorMessage,
                Severity = rule.Severity
            };
        }

        private static Dictionary<string, string> ParseParameters(string? parametersJson)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(parametersJson))
                return result;

            try
            {
                var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(parametersJson);
                if (parsed == null) return result;

                foreach (var kvp in parsed)
                {
                    result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
            catch
            {
            }

            return result;
        }

        private static double ParseDoubleParameter(Dictionary<string, string> parameters, string key, double defaultValue)
        {
            if (parameters.TryGetValue(key, out var val) && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return defaultValue;
        }
    }
}
