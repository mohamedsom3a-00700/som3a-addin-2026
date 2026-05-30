using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace Som3a.Infrastructure.Security;

public static class DataProtection
{
    private const string FormatMarker = "DPv1:";

    [SupportedOSPlatform("windows")]
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("DataProtection is only supported on Windows.");

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return FormatMarker + Convert.ToBase64String(encryptedBytes);
    }

    [SupportedOSPlatform("windows")]
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        if (!encryptedText.StartsWith(FormatMarker, StringComparison.Ordinal))
            return encryptedText;

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("DataProtection is only supported on Windows.");

        var payload = encryptedText[FormatMarker.Length..];
        var encryptedBytes = Convert.FromBase64String(payload);
        var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }

    public static bool IsEncrypted(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.StartsWith(FormatMarker, StringComparison.Ordinal);
    }
}
