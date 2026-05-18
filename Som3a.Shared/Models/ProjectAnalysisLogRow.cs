using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Som3a.Shared.Models
{
    public sealed class ProjectAnalysisLogRow
    {
        public string Time { get; set; } = "";
        public string Level { get; set; } = "INFO";
        public string Message { get; set; } = "";
    }
}
