namespace Som3a.Shared.Models
{
    public sealed class CompareResultItem
    {
        public string TaskId { get; set; }
        public double OldValue { get; set; }
        public double NewValue { get; set; }
        public string ChangeType { get; set; } // Increase / Decrease
        public string Status { get; set; }     // TASK_Up / TASK_Down
        public string WbsId { get; set; }      // from OLD sheet

        public CompareResultItem()
        {
            TaskId = "";
            ChangeType = "";
            Status = "";
            WbsId = "";
        }
    }
}
