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
            services.AddDbContextFactory<RomeDbContext>((sp, options) =>
            {
                options
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseInMemoryDatabase("ROME_IN_MEMORY_DB_CONTEXT_" + Guid.NewGuid().ToString())
                    .ConfigureWarnings((WarningsConfigurationBuilder _) => _.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .UseSeeding((context, _) =>
                    {
                        var ds = sp.GetService<IDataSeeder>();

                        if (ds is not null)
                        {
                            ds.SeedDataAsync(context, CancellationToken.None).GetAwaiter().GetResult();
                        }
                    })
                    .UseAsyncSeeding(async (context, _, cancellationToken) =>
                    {
                        var ds = sp.GetService<IDataSeeder>();

                        if (ds is not null)
                        {
                            await ds.SeedDataAsync(context, cancellationToken);
                        }
                    }); ;
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
            .AddTransient<IDataSeeder>(_ => new RomeDataSeeder(productionMode));
    }
}
