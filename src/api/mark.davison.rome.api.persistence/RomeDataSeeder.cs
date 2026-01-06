namespace mark.davison.rome.api.persistence;

public sealed class RomeDataSeeder : IDataSeeder
{
    private readonly IDateService _dateService;
    private readonly bool _isProductionMode;

    public RomeDataSeeder(
        IDateService dateService,
        bool isProductionMode
    )
    {
        _dateService = dateService;
        _isProductionMode = isProductionMode;
    }

    public async Task SeedDataAsync(DbContext dbContext, CancellationToken token)
    {
        try
        {
            var user = await EnsureUserSeeded(dbContext, token);

            await EnsureTenantSeeded(dbContext, token);
            await EnsureRolesSeeded(dbContext, token);

            await dbContext.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    internal async Task EnsureSeeded<T>(DbContext dbContext, List<T> entities, CancellationToken cancellationToken)
        where T : RomeEntity
    {
        var existingEntities = await dbContext.Set<T>().Where(_ => _.UserId == Guid.Empty).ToListAsync(cancellationToken);

        var newEntities = entities.Where(_ => !existingEntities.Any(e => e.Id == _.Id)).ToList();

        await dbContext.Set<T>().AddRangeAsync(newEntities, cancellationToken);
    }

    private async Task EnsureRolesSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync<Role>(dbContext, _ => _.Id == Guid.Parse("02a740de-569f-4477-b5e7-d8622228db17"), cancellationToken))
        {
            await dbContext.AddAsync(new Role
            {
                Id = Guid.Parse("02a740de-569f-4477-b5e7-d8622228db17"),
                Name = RoleConstants.Admin,
                Description = "Administrator with full access",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                UserId = Guid.Empty
            }, cancellationToken);
        }

        if (!await ExistsAsync<Role>(dbContext, _ => _.Id == Guid.Parse("207af3cb-4a21-4d85-a93d-e16a8690eff2"), cancellationToken))
        {
            // TODO: Constant ids?
            await dbContext.AddAsync(new Role
            {
                Id = Guid.Parse("207af3cb-4a21-4d85-a93d-e16a8690eff2"),
                Name = RoleConstants.User,
                Description = "Standard user with limited access",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                UserId = Guid.Empty
            }, cancellationToken);
        }
    }

    private async Task EnsureTenantSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync<Tenant>(dbContext, _ => _.Id == TenantIds.SystemTenantId, cancellationToken))
        {
            await dbContext.AddAsync(new Tenant
            {
                Id = TenantIds.SystemTenantId,
                Name = "System",
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }, cancellationToken);
        }
    }

    private async Task<User> EnsureUserSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var seededUser = new User
        {
            Id = Guid.Empty,
            TenantId = TenantIds.SystemTenantId,
            Email = "romesystem@markdavison.kiwi",
            DisplayName = "Rome System",
            CreatedAt = _dateService.Now,
            LastModified = _dateService.Now
        };

        var existingUser = await dbContext.Set<User>().FindAsync(Guid.Empty, cancellationToken);

        if (existingUser == null)
        {
            await dbContext.AddAsync(seededUser, cancellationToken);
            existingUser = seededUser;
        }

        return existingUser;

    }

    private async Task<bool> ExistsAsync<TEntity>(
        DbContext dbContext,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken token)
        where TEntity : class
    {
        return await dbContext.Set<TEntity>().Where(predicate).AnyAsync(token);
    }
}
