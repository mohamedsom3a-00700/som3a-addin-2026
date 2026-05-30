using Som3a.Infrastructure.Persistence.SQLite;
using Xunit;

namespace Som3a.Infrastructure.Tests.Repositories;

public class SettingsRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DatabaseFactory _factory;

    public SettingsRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid():N}.db");
        _factory = TestHelpers.CreateFileFactoryAsync(_dbPath).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task SetSetting_And_GetSetting_ReturnsValue()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("Theme", "Mode", "Dark");

        var value = await uow.Settings.GetSettingAsync("Theme", "Mode");

        Assert.Equal("Dark", value);
    }

    [Fact]
    public async Task SetSetting_Upsert_UpdatesExistingValue()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("Theme", "Accent", "#000000");

        await uow.Settings.SetSettingAsync("Theme", "Accent", "#FFFFFF");

        var value = await uow.Settings.GetSettingAsync("Theme", "Accent");
        Assert.Equal("#FFFFFF", value);
    }

    [Fact]
    public async Task GetSetting_NonExistent_ReturnsNull()
    {
        var uow = _factory.CreateUnitOfWork();

        var value = await uow.Settings.GetSettingAsync("NonExistent", "Key");

        Assert.Null(value);
    }

    [Fact]
    public async Task HasCategory_ReturnsTrue_WhenSettingsExist()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("TestCat", "Key1", "Val1");

        var result = await uow.Settings.HasCategoryAsync("TestCat");

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteSetting_RemovesSetting()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("Test", "Key", "Value");

        await uow.Settings.DeleteSettingAsync("Test", "Key");

        var value = await uow.Settings.GetSettingAsync("Test", "Key");
        Assert.Null(value);
    }

    [Fact]
    public async Task GetSettingsByCategory_ReturnsAllInCategory()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("Cat1", "Key1", "Val1");
        await uow.Settings.SetSettingAsync("Cat1", "Key2", "Val2");
        await uow.Settings.SetSettingAsync("Cat2", "Key3", "Val3");

        var results = await uow.Settings.GetSettingsByCategoryAsync("Cat1");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task PluginSettings_AreIsolated()
    {
        var uow = _factory.CreateUnitOfWork();
        await uow.Settings.SetSettingAsync("AI", "ApiKey", "platform-key");
        await uow.Settings.SetSettingAsync("AI", "ApiKey", "plugin-key", "MyPlugin");

        var platform = await uow.Settings.GetSettingAsync("AI", "ApiKey");
        var pluginSettings = await uow.Settings.GetPluginSettingsAsync("MyPlugin");

        Assert.Equal("platform-key", platform);
        Assert.Single(pluginSettings);
        Assert.Equal("plugin-key", pluginSettings[0].Value);
    }

    public void Dispose()
    {
        _factory.Dispose();
        var connStr = $"Data Source={_dbPath};";
        using var clearConn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
        Microsoft.Data.Sqlite.SqliteConnection.ClearPool(clearConn);
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
