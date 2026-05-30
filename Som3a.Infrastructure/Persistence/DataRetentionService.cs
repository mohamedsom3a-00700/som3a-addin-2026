using Som3a.Infrastructure.Persistence.Interfaces;

namespace Som3a.Infrastructure.Persistence;

public class DataRetentionService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly DataRetentionConfiguration _configuration;

    public DataRetentionService(IUnitOfWorkFactory unitOfWorkFactory, DataRetentionConfiguration configuration)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _configuration = configuration;
    }

    public async Task CleanupIfNeededAsync(CancellationToken ct = default)
    {
        var cutoffAi = DateTime.UtcNow - _configuration.AIHistoryRetention;
        var cutoffLogs = DateTime.UtcNow - _configuration.DiagnosticsLogRetention;
        var cutoffCrashes = DateTime.UtcNow - _configuration.CrashReportRetention;
        var cutoffExports = DateTime.UtcNow - _configuration.ExportHistoryRetention;

        using var uow = _unitOfWorkFactory.Create();

        await uow.AI.CleanupAsync(cutoffAi, ct);
        await uow.Logs.CleanupLogsAsync(cutoffLogs, ct);
        await uow.Logs.CleanupCrashesAsync(cutoffCrashes, ct);
        await uow.Logs.CleanupExportHistoryAsync(cutoffExports, ct);
    }
}

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
