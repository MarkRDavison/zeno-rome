namespace mark.davison.rome.api.tests.Framework;

public class RomeApiWebApplicationFactory : WebApplicationFactory<Startup>, ICommonWebApplicationFactory<ApiAppSettings>
{
    public IServiceProvider ServiceProvider => base.Services;
    private readonly Dictionary<string, TestHttpMessageHandler> _messageHandlers = new();

    public RomeApiWebApplicationFactory()
    {

    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) => conf
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.integration.json", false));
        builder.ConfigureTestServices(ConfigureServices);
        builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        });
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHttpClient()
            .AddHttpContextAccessor();

        services.AddScoped<ICurrentUserContext, CurrentUserContext<RomeDbContext>>(_ =>
        {
            var context = new CurrentUserContext<RomeDbContext>(_.GetRequiredService<RomeDbContext>());
            if (ModifyCurrentUserContext != null) { ModifyCurrentUserContext(_, context); }
            return context;
        });

        foreach (var (name, handler) in _messageHandlers)
        {
            services
                .AddHttpClient(name)
                .ConfigurePrimaryHttpMessageHandler(_ => handler);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var dbSettings = Services.GetRequiredService<IOptions<DatabaseAppSettings>>();

        }

        base.Dispose(disposing);
    }

    public UserInfo UserInfo => new(
        "test@gmail.com",
        "Keycloak",
        Guid.NewGuid().ToString(),
        "Test User",
        Guid.NewGuid(),
        TenantIds.SystemTenantId,
        ["User", "Admin"]);

    public TestHttpMessageHandler GetMessageHandler(string httpClientName) => _messageHandlers[httpClientName];

    public Action<IServiceProvider, CurrentUserContext<RomeDbContext>>? ModifyCurrentUserContext { get; set; }
}