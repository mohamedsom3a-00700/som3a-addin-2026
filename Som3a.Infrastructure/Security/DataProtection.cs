using System.Security.Cryptography;

namespace Som3a.Infrastructure.Security;

public static class DataProtection
{
    private const string FormatMarker = "DPv1:";

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return FormatMarker + Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        if (!encryptedText.StartsWith(FormatMarker, StringComparison.Ordinal))
            return encryptedText;

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
