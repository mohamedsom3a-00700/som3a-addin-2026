namespace Som3a.Infrastructure.Persistence.SQLite;

public class SQLiteConfiguration
{
    public string DataDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Som3a");

    public string FileName { get; set; } = "platform.db";

    public string DatabasePath => Path.Combine(DataDirectory, FileName);

    public string ConnectionString => $"Data Source={DatabasePath};";

    public int BusyTimeoutMs { get; set; } = 5000;

    public bool EnableWAL { get; set; } = true;

    public bool EnableForeignKeys { get; set; } = true;
}
