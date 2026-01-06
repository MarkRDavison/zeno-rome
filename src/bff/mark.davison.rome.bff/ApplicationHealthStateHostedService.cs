namespace mark.davison.rome.bff;

public sealed class ApplicationHealthStateHostedService : GenericApplicationHealthStateHostedService<BffAppSettings>
{
    public ApplicationHealthStateHostedService(
        IApplicationHealthState applicationHealthState,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<BffAppSettings> appSettings
    ) : base(
        applicationHealthState,
        hostApplicationLifetime,
        appSettings)
    {
    }
}
