namespace Som3a.Infrastructure.Persistence.Models;

public class BackupManifest
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string IncludedTables { get; set; } = "[]";
    public string PlatformVersion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsAutoBackup { get; set; }
}
