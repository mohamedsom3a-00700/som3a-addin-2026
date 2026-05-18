using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace Som3a.Shared.Models
{
    public class WbsItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public int WbsLevel { get; set; }
        public string WbsName { get; set; }
        public ObservableCollection<object> Items { get; set; } = new();  // 🔥 المهم

        public string FullPath { get; set; }
        public bool IsCritical => false;
        public string WbsCode { get; set; }
        public string Display => $"{FullPath} - {WbsName}";

    }
}
