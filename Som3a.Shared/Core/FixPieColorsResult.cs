using System.Collections.Generic;

namespace Som3a.Shared.Core
{
    public sealed class FixPieColorsResult
    {
        public int ChartsCount { get; set; }
        public int SeriesCount { get; set; }
        public int PointsTotal { get; set; }
        public int Matched { get; set; }
        public int Updated { get; set; }
        public List<string> SampleLabels { get; } = new List<string>();
        public List<string> NotMatchedLabels { get; } = new List<string>();
    }
}
