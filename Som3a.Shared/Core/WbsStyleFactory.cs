using Som3a.Shared.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Som3a.Shared.Core
{
    public static class WbsStyleFactory
    {
        // ================= DEFAULT =================
        private static IReadOnlyDictionary<int, WbsLevelStyle> Default()
        {
            return WbsLevelStyle.Default15();
        }

        // ================= BLUE GRADIENT =================
        private static IReadOnlyDictionary<int, WbsLevelStyle> BlueGradient()
        {
            return GenerateGradient(
                Color.FromArgb(220, 235, 255),
                Color.FromArgb(0, 70, 160)
            );
        }

        // ================= DARK MODE =================
        private static IReadOnlyDictionary<int, WbsLevelStyle> DarkMode()
        {
            return GenerateGradient(
                Color.FromArgb(60, 60, 60),
                Color.FromArgb(10, 10, 10)
            );
        }

        // ================= SOFT PASTEL =================
        private static IReadOnlyDictionary<int, WbsLevelStyle> SoftPastel()
        {
            return GenerateGradient(
                Color.FromArgb(255, 240, 245),
                Color.FromArgb(200, 220, 255)
            );
        }

        // ================= GRADIENT ENGINE =================
        private static Dictionary<int, WbsLevelStyle> GenerateGradient(Color start, Color end)
        {
            var dict = new Dictionary<int, WbsLevelStyle>();

            for (int i = 1; i <= 15; i++)
            {
                double t = (i - 1) / 14.0;

                Color c = Color.FromArgb(
                    (int)(start.R + (end.R - start.R) * t),
                    (int)(start.G + (end.G - start.G) * t),
                    (int)(start.B + (end.B - start.B) * t)
                );

                dict[i] = new WbsLevelStyle(c, WbsLevelStyle.PickReadableFont(c))
                {
                    Level = i
                };
            }

            return dict;
        }

        // ================= PRIMAVERA STYLE =================
        private static IReadOnlyDictionary<int, WbsLevelStyle> PrimaveraLike()
        {
            var dict = new Dictionary<int, WbsLevelStyle>();

            dict[1] = new WbsLevelStyle(Color.FromArgb(255, 255, 204), WbsLevelStyle.PickReadableFont(Color.FromArgb(255, 255, 204)));
            dict[2] = new WbsLevelStyle(Color.FromArgb(204, 255, 204), WbsLevelStyle.PickReadableFont(Color.FromArgb(204, 255, 204)));
            dict[3] = new WbsLevelStyle(Color.FromArgb(204, 229, 255), WbsLevelStyle.PickReadableFont(Color.FromArgb(204, 229, 255)));
            dict[4] = new WbsLevelStyle(Color.FromArgb(255, 204, 229), WbsLevelStyle.PickReadableFont(Color.FromArgb(255, 204, 229)));
            dict[5] = new WbsLevelStyle(Color.FromArgb(240, 240, 240), WbsLevelStyle.PickReadableFont(Color.FromArgb(240, 240, 240)));

            // fallback لباقي المستويات
            for (int i = 6; i <= 15; i++)
            {
                dict[i] = dict[5];
            }

            return dict;
        }

        // ================= MAIN SWITCH =================
        public static IReadOnlyDictionary<int, WbsLevelStyle> GetStyle(int styleId)
        {
            switch (styleId)
            {
                case 1: return Default();
                case 2: return BlueGradient();
                case 3: return PrimaveraLike();
                case 4: return DarkMode();
                case 5: return SoftPastel();
                default: return Default();
            }
        }
    }
        public static class UserSettings
        {
            public static int SelectedStyle { get; set; } = 1;
        }
}