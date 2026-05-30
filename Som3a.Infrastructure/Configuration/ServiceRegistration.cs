using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Som3a.Infrastructure.Persistence;
using Som3a.Infrastructure.Persistence.Interfaces;
using Som3a.Infrastructure.Persistence.SQLite;

namespace Som3a.Infrastructure.Configuration;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, SQLiteConfiguration? configuration = null)
    {
        configuration ??= new SQLiteConfiguration();

        services.AddSingleton(configuration);
        services.AddSingleton<DatabaseFactory>();
        services.AddSingleton<IUnitOfWorkFactory>(sp =>
        {
            var factory = sp.GetRequiredService<DatabaseFactory>();
            return new UnitOfWorkFactory(factory);
        });
        services.AddTransient<IUnitOfWork>(sp =>
        {
            var factory = sp.GetRequiredService<DatabaseFactory>();
            return factory.CreateUnitOfWork();
        });
        services.AddTransient<ISettingsRepository>(sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            return uow.Settings;
        });
        services.AddTransient<IAIRepository>(sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            return uow.AI;
        });
        services.AddTransient<IPluginRepository>(sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            return uow.Plugins;
        });
        services.AddTransient<ILogRepository>(sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            return uow.Logs;
        });
        services.AddTransient<ITemplateRepository>(sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            return uow.Templates;
        });
        services.AddSingleton<DataRetentionConfiguration>();
        services.AddTransient<DataRetentionService>();

        return services;
    }
}

internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DatabaseFactory _databaseFactory;

    public UnitOfWorkFactory(DatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public IUnitOfWork Create() => _databaseFactory.CreateUnitOfWork();
}
