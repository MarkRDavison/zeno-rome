namespace mark.davison.rome.api.queries.Scenarios.Startup;

public sealed class BootstrapQueryProcessor : IQueryProcessor<BootstrapQueryRequest, BootstrapQueryResponse>
{
    private readonly IOptions<AuthenticationSettings> _authSettings;

    public BootstrapQueryProcessor(IOptions<AuthenticationSettings> authSettings)
    {
        _authSettings = authSettings;
    }

    public Task<BootstrapQueryResponse> ProcessAsync(BootstrapQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        return Task.FromResult(new BootstrapQueryResponse
        {
            Value = new BootstrapDataDto([.. _authSettings.Value.PROVIDERS.Select(_ => _.Name)])
        });
    }
}
