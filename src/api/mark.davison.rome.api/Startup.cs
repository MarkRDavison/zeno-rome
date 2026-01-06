namespace mark.davison.rome.api;

[UseCQRSServer]
public class Startup(IConfiguration Configuration)
{
    public ApiAppSettings AppSettings { get; set; } = new();

    public void ConfigureServices(IServiceCollection services)
    {
        AppSettings = services.BindAppSettings(Configuration);

        services
            .AddCors(o =>
            {
                o.AddDefaultPolicy(builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            })
            .AddLogging()
            .AddAuthorization()
            .AddJwtAuthentication<RomeDbContext>(AppSettings.AUTHENTICATION)
            .AddCoreDbContext<RomeDbContext>()
            .AddHealthCheckServices<ApplicationHealthStateHostedService>()
            .AddDatabase<RomeDbContext>(AppSettings.PRODUCTION_MODE, AppSettings.DATABASE, typeof(PostgresContextFactory), typeof(SqliteContextFactory))
            .AddServerCore()
            .AddCQRSServer()
            .AddTransient<IDataSeeder>(_ => new RomeDataSeeder(_.GetRequiredService<IDateService>(), AppSettings.PRODUCTION_MODE));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app
            .UseHttpsRedirection()
            .UseRouting()
            .UseCors()
            .UseMiddleware<RequestResponseLoggingMiddleware>()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints
                    .MapBackendRemoteAuthenticationEndpoints<RomeDbContext>()
                    .MapCQRSEndpoints()
                    .MapCommonHealthChecks();
            });
    }
}
