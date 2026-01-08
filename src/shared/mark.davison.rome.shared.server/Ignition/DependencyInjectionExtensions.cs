namespace mark.davison.rome.shared.server.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddScoped<IFinanceUserContext, FinanceUserContext>();

        return services;
    }
}
