using System.Text.RegularExpressions;

namespace Som3a.Infrastructure.Persistence.Validation;

public static class EntityValidator
{
    private static readonly HashSet<string> ValidSeverities = new(StringComparer.OrdinalIgnoreCase)
    { "Debug", "Info", "Warning", "Error", "Fatal" };

    private static readonly HashSet<string> ValidTemplateTypes = new(StringComparer.OrdinalIgnoreCase)
    { "WBS", "Activity", "Relationship", "ProductivityBenchmark" };

    private static readonly Regex SemverRegex = new(
        @"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

    private static readonly Regex Sha256Regex = new(
        @"^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

    public static void ValidateGuid(Guid id, string fieldName)
    {
        if (id == Guid.Empty)
            throw new ValidationException($"{fieldName} must be a non-empty GUID.");
    }

    public static void ValidateMaxLength(string? value, int maxLength, string fieldName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ValidationException($"{fieldName} exceeds maximum length of {maxLength} characters (was {value.Length}).");
    }

    public static void ValidateSeverity(string severity)
    {
        if (!ValidSeverities.Contains(severity))
            throw new ValidationException($"Invalid severity '{severity}'. Must be one of: {string.Join(", ", ValidSeverities)}.");
    }

    public static void ValidateTemplateType(string templateType)
    {
        if (!ValidTemplateTypes.Contains(templateType))
            throw new ValidationException($"Invalid template type '{templateType}'. Must be one of: {string.Join(", ", ValidTemplateTypes)}.");
    }

    public static void ValidateSemver(string version)
    {
        if (!SemverRegex.IsMatch(version))
            throw new ValidationException($"Invalid semantic version '{version}'. Must match 'major.minor.patch' format.");
    }

    public static void ValidateChecksum(string checksum)
    {
        if (!Sha256Regex.IsMatch(checksum))
            throw new ValidationException($"Invalid checksum format. Must be a 64-character hexadecimal string.");
    }

    public static void ValidateAbsolutePath(string path, string fieldName)
    {
        if (!Path.IsPathRooted(path))
            throw new ValidationException($"{fieldName} must be an absolute path.");
    }

    public static void ValidateNonNegative(int value, string fieldName)
    {
        if (value < 0)
            throw new ValidationException($"{fieldName} must be non-negative.");
    }

    public static void ValidateStringNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{fieldName} must not be empty.");
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
