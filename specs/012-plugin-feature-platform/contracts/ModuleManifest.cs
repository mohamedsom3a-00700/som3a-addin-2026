/// <summary>
/// Module metadata stored in module.json alongside each module assembly.
/// Used for discovery and integrity validation.
/// </summary>
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
