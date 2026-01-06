namespace mark.davison.rome.api.persistence.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        bool productionMode,
        DatabaseAppSettings databaseSettings,
        params Type[] migrationTypes)
    {
        return services
            .AddDatabase<RomeDbContext>(productionMode, databaseSettings, migrationTypes)
            .AddCoreDbContext<RomeDbContext>()
            .AddTransient<IDataSeeder>(_ => new RomeDataSeeder(_.GetRequiredService<IDateService>(), productionMode));
    }
}
