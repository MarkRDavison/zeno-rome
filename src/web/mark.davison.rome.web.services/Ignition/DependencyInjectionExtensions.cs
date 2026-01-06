namespace mark.davison.rome.web.services.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseRomeServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IDateService>(_ => new DateService(DateService.DateMode.Local));
    }
}