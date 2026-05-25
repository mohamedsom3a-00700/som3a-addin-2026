namespace Som3a_WPF_UI.Contracts
{
    public class ModuleManifest
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Hash { get; set; }
        public string HashAlgorithm { get; set; } = "SHA256";
        public string[] Capabilities { get; set; }
        public string[] Dependencies { get; set; }
    }
}
