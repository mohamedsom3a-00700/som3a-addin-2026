using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Som3a.Shared.Models
{
    public sealed class ProjectAnalysisSummaryRow
    {
        public string Type { get; set; } = "";
        public int Count { get; set; }
        public string SheetName { get; set; } = "";
        public string Details { get; set; } = "";
    }
}
