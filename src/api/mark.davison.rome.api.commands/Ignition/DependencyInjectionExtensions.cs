namespace mark.davison.rome.api.commands.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRomeCommands(this IServiceCollection services)
    {
        return services
            .AddTransient<ICreateTransactionValidatorStrategyFactory, CreateTransactionValidatorStrategyFactory>()
            .AddTransient<ICreateTransctionValidationContext, CreateTransctionValidationContext>();
    }
}
