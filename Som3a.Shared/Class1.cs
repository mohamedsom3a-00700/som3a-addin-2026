using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Som3a.Shared.Models
{
    public sealed class PrevFileItem
    {
        public string DisplayName { get; set; } = "";
        public string FullPath { get; set; } = "";
    }

    public sealed class NamePickItem
    {
        public bool IsChecked { get; set; } = true;
        public string NameRaw { get; set; } = "";
        public int Row { get; set; }
        public string NormKey { get; set; } = "";
    }

    public sealed class PreviewRow
    {
        public string Name { get; set; } = "";
        public double Previous { get; set; }
        public double Today { get; set; }
        public double Var => Today - Previous;
        public double Total => Today + Previous;
        public bool IsTotals { get; set; }
    }
}
