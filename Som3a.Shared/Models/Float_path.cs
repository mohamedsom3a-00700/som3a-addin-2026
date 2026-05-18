using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Som3a.Shared.Models
{
    public class Activity
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int WbsLevel { get; set; }
        public string WbsName { get; set; }

        public double TotalFloat { get; set; }

        public string WBSId { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public double BudgetCost { get; set; }


        public string DisplayActivity => $"{Code} - {Name}";

        // 🔥 مهم عشان ComboBox
        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }

    public class Relationship
    {
        public string PredecessorId { get; set; }
        public string SuccessorId { get; set; }

        public string Type { get; set; } // FS, SS, FF, SF
        public double Lag { get; set; }
    }

    public class PathResult
    {
        public List<Activity> Activities { get; set; } = new();
        public double PathFloat { get; set; }
    }
}