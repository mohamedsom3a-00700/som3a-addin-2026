using System;

namespace Som3a.Shared.Utils
{
    public static class RelUtils
    {
        public static string BuildTripleKey(object? pred, object? succ, object? predType)
            => $"{NormalizeStr(pred)}||{NormalizeStr(succ)}||{NormalizeStr(predType)}";

        public static string NormalizeStr(object? v)
        {
            try
            {
                if (v == null) return "";
                var s = Convert.ToString(v) ?? "";
                s = s.Replace((char)160, ' '); // NBSP
                s = s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                s = s.Trim();
                while (s.Contains("  ")) s = s.Replace("  ", " ");
                return s.ToLowerInvariant();
            }
            catch
            {
                return "";
            }
        }
    }
}
