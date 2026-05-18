using System.Collections.Generic;
using System.Drawing;

namespace Som3a.Shared.Models
{
    public sealed class WbsLevelStyle
    {
        public int Level { get; set; }
        public Color Fill { get; set; }
        public Color Font { get; set; }

        // ✔ Constructor مهم
        public WbsLevelStyle() { }

        public WbsLevelStyle(Color fill, Color font)
        {
            Fill = fill;
            Font = font;
        }

        public static IReadOnlyDictionary<int, WbsLevelStyle> Default15()
        {
            var map = new Dictionary<int, WbsLevelStyle>(15);

            Add(map, 1, "#FFFF00");
            Add(map, 2, "#92D050");
            Add(map, 3, "#0066CC");
            Add(map, 4, "#FF0000");
            Add(map, 5, "#B2B2B2");
            Add(map, 6, "#C00000");
            Add(map, 7, "#FFC000");
            Add(map, 8, "#00B0F0");
            Add(map, 9, "#002060");
            Add(map, 10, "#66AA00");
            Add(map, 11, "#994499");
            Add(map, 12, "#22AA99");
            Add(map, 13, "#AAAA11");
            Add(map, 14, "#6633CC");
            Add(map, 15, "#E67300");

            return map;
        }

        private static void Add(Dictionary<int, WbsLevelStyle> map, int lvl, string hex)
        {
            var fill = ColorTranslator.FromHtml(hex);
            var font = PickReadableFont(fill);

            map[lvl] = new WbsLevelStyle(fill, font)
            {
                Level = lvl
            };
        }

        // ✔ خلتها public عشان نستخدمها في Factory
        public static Color PickReadableFont(Color bg)
        {
            var lum = (0.2126 * bg.R) + (0.7152 * bg.G) + (0.0722 * bg.B);
            return lum < 140 ? Color.White : Color.Black;
        }
    }
}