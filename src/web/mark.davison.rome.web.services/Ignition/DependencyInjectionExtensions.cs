namespace mark.davison.rome.web.services.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseRomeServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IStartupState, StartupState>()
            .AddScoped<IAppSessionBootstrap, AppSessionBootstrap>()
            .AddScoped<IAccountState, AccountState>()
            .AddScoped<IHomeState, HomeState>()
            .AddScoped<ITransactionState, TransactionState>()
            .AddScoped<ICategoryState, CategoryState>()
            .AddScoped<ITagState, TagState>()
            .AddScoped<IAppContextService, AppContextService>()
            .AddScoped<IDateService>(_ => new DateService(DateService.DateMode.Local));
    }
}