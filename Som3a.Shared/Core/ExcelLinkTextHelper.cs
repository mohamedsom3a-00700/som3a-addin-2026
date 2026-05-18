using System;

namespace Som3a.Shared.Core
{
    public static class ExcelLinkTextHelper
    {
        public static string GetExternalLinkFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // استخدم string overload بدل char overload
            int p = text.IndexOf("[", StringComparison.Ordinal);
            if (p < 0) return "";

            int q = text.IndexOf("]", p + 1, StringComparison.Ordinal);
            if (q < 0) return "";

            int start = Math.Max(0, p - 1);
            int len = Math.Min(text.Length - start, (q - p + 2));
            if (len <= 0) return "";

            return text.Substring(start, len).Trim();
        }
    }
}
