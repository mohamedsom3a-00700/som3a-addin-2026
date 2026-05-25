namespace Som3a.Localization.RTL
{
    public static class RTLHelper
    {
        public static bool IsRTLCulture(string cultureName)
        {
            if (cultureName == null)
                throw new ArgumentNullException(nameof(cultureName));
            if (string.IsNullOrWhiteSpace(cultureName))
                throw new ArgumentException("Culture name must not be empty or whitespace.", nameof(cultureName));

            try
            {
                var culture = new System.Globalization.CultureInfo(cultureName);
                return culture.TextInfo.IsRightToLeft;
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                return false;
            }
        }
    }
}
