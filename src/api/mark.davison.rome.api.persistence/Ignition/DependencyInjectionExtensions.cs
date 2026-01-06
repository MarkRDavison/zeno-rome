namespace mark.davison.rome.api.persistence.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        bool productionMode,
        DatabaseAppSettings databaseSettings,
        params Type[] migrationTypes)
    {
        if (databaseSettings.CONNECTION_STRING == "MEMORY")
        {
            services.AddDbContextFactory<RomeDbContext>(options =>
            {
                options
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseInMemoryDatabase("ROME_IN_MEMORY_DB_CONTEXT_" + Guid.NewGuid().ToString())
                    .ConfigureWarnings((WarningsConfigurationBuilder _) => _.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
            services.AddScoped<IDbContext<RomeDbContext>>(_ => _.GetRequiredService<RomeDbContext>());
        }
        else
        {
            services
                .AddDatabase<RomeDbContext>(productionMode, databaseSettings, migrationTypes);
        }

        return services
            .AddCoreDbContext<RomeDbContext>()
            .AddTransient<IDataSeeder>(_ => new RomeDataSeeder(_.GetRequiredService<IDateService>(), productionMode));
    }
}
