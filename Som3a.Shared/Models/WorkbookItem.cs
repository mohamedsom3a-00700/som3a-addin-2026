namespace Som3a.Shared.Models
{
    public sealed class WorkbookItem
    {
        public string Name { get; }
        public WorkbookItem(string name) => Name = name ?? "";
        public override string ToString() => Name;
    }
}
