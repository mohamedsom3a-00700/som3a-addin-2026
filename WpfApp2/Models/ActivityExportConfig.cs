namespace Som3a_WPF_UI.Models
{
    public class ActivityExportConfig
    {
        public string TargetSheetName { get; set; } = "Generated Activities";
        public string[] Columns { get; set; } = {
            "ActivityId", "Name", "Description", "BOQReference",
            "Quantity", "Unit", "Dependencies"
        };
        public bool IncludeDependencies { get; set; } = true;
        public bool OverwriteExisting { get; set; }
        public object? ThemeColors { get; set; }
    }
}
