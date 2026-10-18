using mark.davison.rome.shared.models.dto.Scenarios.Queries.Startup;

namespace mark.davison.rome.web.services.State;

internal sealed class AppSessionBootstrap : IAppSessionBootstrap
{
    private readonly IClientHttpRepository _clientRepository;
    private readonly IStartupState _startupState;
    private readonly IAppContextService _appContextService;
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    public AppSessionBootstrap(
        IClientHttpRepository clientRepository,
        IStartupState startupState,
        IAppContextService appContextService)
    {
        _clientRepository = clientRepository;
        _startupState = startupState;
        _appContextService = appContextService;
    }

    public async Task EnsureAuthenticatedStartupAsync(CancellationToken cancellationToken = default)
    {
        if (_startupState.ReferenceDataLoaded)
        {
            return;
        }

        await _loadGate.WaitAsync(cancellationToken);
        try
        {
            if (_startupState.ReferenceDataLoaded)
            {
                return;
            }

            var startupResponse = await _clientRepository.Get<StartupQueryRequest, StartupQueryResponse>(CancellationToken.None);

            if (startupResponse.SuccessWithValue)
            {
                _startupState.NotifyStateChanged(startupResponse.Value);
                _appContextService.UpdateRange(
                    startupResponse.Value.UserContext.StartRange,
                    startupResponse.Value.UserContext.EndRange);
            }
        }
        finally
        {
            _loadGate.Release();
        }
    }
}
