using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Som3a.Shared.Models
{
    public sealed class SheetCheckRow
    {
        public string Section { get; set; } = "";        // Sheet/Section
        public string RequiredColumn { get; set; } = "";  // Required Column
        public string HeaderFound { get; set; } = "";     // Yes/No
        public string Location { get; set; } = "";        // e.g. "Row 1 Col 7"
    }
}
