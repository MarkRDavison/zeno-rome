namespace mark.davison.rome.bff;

public class Startup(IConfiguration Configuration)
{
    public BffAppSettings AppSettings { get; set; } = new();

    public void ConfigureServices(IServiceCollection services)
    {
        AppSettings = services.BindAppSettings(Configuration);

        services
            .Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            })
            .AddCors(o =>
            {
                o.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(AppSettings.WEB_ORIGIN)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            })
            .AddLogging()
            .AddServerCore()
            .AddScoped<IUserAuthenticationService, RemoteUserAuthenticationService>()
            .AddRedis(AppSettings.REDIS, AppSettings.PRODUCTION_MODE)
            .AddRemoteForwarderAuthentication(AppSettings.API_ORIGIN)
            .AddHealthCheckServices<ApplicationHealthStateHostedService>()
            .AddOidcCookieAuthentication(
                AppSettings.AUTHENTICATION,
                (s, email, name) =>
                {
                    var now = s.GetRequiredService<IDateService>().Now;
                    return new UserDto(
                        Guid.NewGuid(),
                        TenantIds.SystemTenantId,
                        email,
                        name,
                        true,
                        now,
                        now);
                });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app
            .UseForwardedHeaders()
            .UseHttpsRedirection()
            .UseRouting()
            .UseCors()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints
                    .MapInteractiveAuthenticationEndpoints(AppSettings.WEB_ORIGIN)
                    .UseApiProxy(AppSettings.API_ORIGIN)
                    .MapCommonHealthChecks();
            });
    }
}
