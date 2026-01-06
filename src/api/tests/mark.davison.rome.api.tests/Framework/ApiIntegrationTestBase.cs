namespace mark.davison.rome.api.tests.Framework;

public class ApiIntegrationTestBase : IntegrationTestBase<RomeApiWebApplicationFactory, ApiAppSettings>
{
    private IServiceScope? _serviceScope;
    public ApiIntegrationTestBase()
    {
        _serviceScope = Services.CreateScope();
        _factory.ModifyCurrentUserContext = (serviceProvider, currentUserContext) =>
        {
            var appSettings = serviceProvider.GetRequiredService<IOptions<ApiAppSettings>>();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Email, UserInfo.Email));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserInfo.Sub));
            identity.AddClaim(new Claim("sub", UserInfo.Sub));
            currentUserContext.PopulateFromPrincipal(new ClaimsPrincipal(identity), UserInfo.Provider);
        };
    }

    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        await base.SeedData(serviceProvider);
        using var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedDataAsync(scope.ServiceProvider.GetRequiredService<RomeDbContext>(), CancellationToken.None);
        await SeedCoreData(scope.ServiceProvider);
        await SeedTestData(scope.ServiceProvider);
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        if (_serviceScope == null)
        {
            throw new NullReferenceException();
        }
        return _serviceScope!.ServiceProvider.GetRequiredService<T>();
    }

    protected virtual Task SeedTestData(IServiceProvider services) => Task.CompletedTask;

    private async Task SeedCoreData(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<IDbContext<RomeDbContext>>();

        var user = new User
        {
            Id = UserInfo.UserId,
            TenantId = UserInfo.TenantId,
            Email = UserInfo.Email,
            DisplayName = UserInfo.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            Provider = UserInfo.Provider,
            ProviderSubject = UserInfo.Sub,
            UserId = UserInfo.UserId,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var roles = await dbContext
            .Set<Role>()
            .AsNoTracking()
            .Where(_ => UserInfo.UserRoles.Contains(_.Name))
            .ToListAsync();

        var userRoles = UserInfo.UserRoles
            .Select(_ => new UserRole
            {
                Id = Guid.NewGuid(),
                RoleId = roles.Single(r => r.Name == _).Id,
                UserId = UserInfo.UserId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            })
            .ToList();

        user.ExternalLogins.Add(externalLogin);
        userRoles.ForEach(user.UserRoles.Add);

        await dbContext.AddAsync(user, CancellationToken.None);
        await dbContext.AddAsync(externalLogin, CancellationToken.None);
        foreach (var ur in userRoles)
        {
            await dbContext.AddAsync(ur, CancellationToken.None);
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public UserInfo UserInfo => _factory.UserInfo;
}
