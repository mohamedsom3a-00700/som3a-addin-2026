namespace Som3a.Shared.Models
{
    public sealed class LinkTypeItem
    {
        public string Key { get; }
        public string DisplayName { get; }

        public LinkTypeItem(string key, string? displayName = null)
        {
            Key = key ?? "";
            DisplayName = displayName ?? key ?? "";
        }

        public override string ToString() => DisplayName;
    }
}
