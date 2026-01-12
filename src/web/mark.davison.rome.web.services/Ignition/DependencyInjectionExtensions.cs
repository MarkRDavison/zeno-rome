namespace mark.davison.rome.web.services.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseRomeServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IStartupState, StartupState>()
            .AddScoped<IAccountState, AccountState>()
            .AddScoped<IAppContextService, AppContextService>()
            .AddScoped<IDateService>(_ => new DateService(DateService.DateMode.Local));
    }
}