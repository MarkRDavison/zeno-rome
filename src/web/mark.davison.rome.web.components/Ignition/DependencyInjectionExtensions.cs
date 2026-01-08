namespace mark.davison.rome.web.components.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseRomeComponents(this IServiceCollection services)
    {
        services
            .UseClientRepository(WebConstants.ApiClientName, WebConstants.LocalBffRoot)
            .UseAuthentication(WebConstants.ApiClientName, WebConstants.LocalBffRoot)
            .UseClientCQRS(typeof(Routes))
            .UseCommonClient(typeof(Routes));

        services
            .AddScoped<AccountListViewModel>();

        return services;
    }
}
