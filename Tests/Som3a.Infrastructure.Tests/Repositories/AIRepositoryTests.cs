using Microsoft.Data.Sqlite;
using Som3a.Infrastructure.Persistence.Models;
using Som3a.Infrastructure.Persistence.SQLite;
using Xunit;

namespace Som3a.Infrastructure.Tests.Repositories;

public class AIRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DatabaseFactory _factory;

    public AIRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_ai_{Guid.NewGuid():N}.db");
        _factory = TestHelpers.CreateFileFactoryAsync(_dbPath).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task LogExecution_And_GetExecution_ReturnsRecord()
    {
        var uow = _factory.CreateUnitOfWork();
        var record = new AIExecutionRecord
        {
            Id = Guid.NewGuid(),
            ProviderName = "Claude",
            ModelName = "claude-3-opus",
            PromptText = "Generate BOQ",
            ResponseText = "Generated",
            TokenInput = 100,
            TokenOutput = 50,
            DurationMs = 2000,
            Status = "Success",
            ExecutedAt = DateTime.UtcNow
        };

        await uow.AI.LogExecutionAsync(record);
        var retrieved = await uow.AI.GetExecutionAsync(record.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Claude", retrieved!.ProviderName);
        Assert.Equal(100, retrieved.TokenInput);
        Assert.Equal(50, retrieved.TokenOutput);
    }

    [Fact]
    public async Task QueryExecutions_WithProviderFilter_ReturnsFiltered()
    {
        var uow = _factory.CreateUnitOfWork();
        var now = DateTime.UtcNow;

        await uow.AI.LogExecutionAsync(new AIExecutionRecord
        { Id = Guid.NewGuid(), ProviderName = "OpenAI", ModelName = "gpt-4", PromptText = "A", ResponseText = "B", TokenInput = 1, TokenOutput = 1, DurationMs = 100, Status = "Success", ExecutedAt = now });
        await uow.AI.LogExecutionAsync(new AIExecutionRecord
        { Id = Guid.NewGuid(), ProviderName = "Claude", ModelName = "claude-3", PromptText = "A", ResponseText = "B", TokenInput = 1, TokenOutput = 1, DurationMs = 100, Status = "Success", ExecutedAt = now });

        var openaiResults = await uow.AI.QueryExecutionsAsync(providerName: "OpenAI");

        Assert.Single(openaiResults);
        Assert.Equal("OpenAI", openaiResults[0].ProviderName);
    }

    [Fact]
    public async Task CountExecutions_ReturnsCorrectCount()
    {
        var uow = _factory.CreateUnitOfWork();
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
        {
            await uow.AI.LogExecutionAsync(new AIExecutionRecord
            { Id = Guid.NewGuid(), ProviderName = "DeepSeek", ModelName = "deepseek-v3", PromptText = "P", ResponseText = "R", TokenInput = 1, TokenOutput = 1, DurationMs = 50, Status = "Success", ExecutedAt = now });
        }

        var count = await uow.AI.CountExecutionsAsync();

        Assert.Equal(5, count);
    }

    public void Dispose()
    {
        _factory.Dispose();
        var connStr = $"Data Source={_dbPath};";
        using var clearConn = new SqliteConnection(connStr);
        SqliteConnection.ClearPool(clearConn);
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
