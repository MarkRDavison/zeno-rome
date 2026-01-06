namespace mark.davison.rome.api;

public class ApplicationHealthStateHostedService(
    IApplicationHealthState applicationHealthState,
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<ApiAppSettings> appSettings,
    IDbContextFactory<RomeDbContext> dbContextFactory
) : ApiApplicationHealthStateHostedService<RomeDbContext, ApiAppSettings>(
    applicationHealthState,
    hostApplicationLifetime,
    appSettings,
    dbContextFactory)
{
    protected override async Task InitDatabaseProduction(RomeDbContext dbContext, CancellationToken cancellationToken)
    {
        if (PendingModelChangesChecker.HasPendingModelChanges(dbContext))
        {
            var details = PendingModelChangesChecker.GetPendingModelChanges(dbContext);
            Console.Error.WriteLine(details);
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    protected override async Task InitDatabaseDevelopment(RomeDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}