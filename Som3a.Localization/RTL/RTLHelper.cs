namespace Som3a.Localization.RTL
{
    public static class RTLHelper
    {
        public static bool IsRTLCulture(string cultureName)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo(cultureName);
                return culture.TextInfo.IsRightToLeft;
            }
            catch
            {
                return false;
            }
        }
    }
}
