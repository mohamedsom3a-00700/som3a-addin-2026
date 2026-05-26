using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Som3a_WPF_UI.Services
{
    public class FontFamilyInfo
    {
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public bool IsArabicCompatible { get; set; }
        public FontFamily FontFamily { get; set; }
    }

    public static class FontEnumerator
    {
        public static List<FontFamilyInfo> GetSystemFonts()
        {
            var fonts = new List<FontFamilyInfo>();

            try
            {
                foreach (var family in Fonts.SystemFontFamilies)
                {
                    var info = new FontFamilyInfo
                    {
                        Name = family.Source,
                        FamilyName = family.Source,
                        FontFamily = family,
                        IsArabicCompatible = IsArabicCompatible(family)
                    };
                    fonts.Add(info);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FontEnumerator] Error enumerating fonts: {ex.Message}");
            }

            return fonts.OrderBy(f => f.FamilyName).ToList();
        }

        public static bool IsArabicCompatible(FontFamily fontFamily)
        {
            try
            {
                var typefaces = fontFamily.GetTypefaces();
                foreach (var typeface in typefaces)
                {
                    if (typeface.TryGetGlyphTypeface(out var glyphTypeface))
                    {
                        for (int cp = 0x0600; cp <= 0x06FF; cp++)
                        {
                            if (glyphTypeface.CharacterToGlyphMap.TryGetValue(cp, out _))
                                return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public static BitmapSource GeneratePreview(FontFamily fontFamily, string sampleText)
        {
            if (string.IsNullOrEmpty(sampleText))
                sampleText = "AaBbCc";

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var typeface = new Typeface(
                    fontFamily,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);

                var formattedText = new FormattedText(
                    sampleText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    16,
                    Brushes.Black,
                    1.0);

                dc.DrawRectangle(Brushes.White, null, new System.Windows.Rect(0, 0, 200, 40));
                dc.DrawText(formattedText, new System.Windows.Point(4, 4));
            }

            var renderBitmap = new RenderTargetBitmap(200, 40, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(visual);
            if (renderBitmap.CanFreeze) renderBitmap.Freeze();
            return renderBitmap;
        }
    }
}
