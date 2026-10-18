using mark.davison.rome.shared.models.dto.Scenarios.Queries.Home;

namespace mark.davison.rome.web.services.State.Home;

internal sealed class HomeState : IHomeState
{
    private readonly IClientHttpRepository _clientRepository;

    public HomeState(IClientHttpRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    public HomeQueryValue? Home { get; private set; }

    public event EventHandler StateChanged = default!;

    public void NotifyStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);

    public async Task FetchState()
    {
        Home = null;
        Loading = true;
        Loaded = false;
        NotifyStateChanged();

        var response = await _clientRepository.Get<HomeQueryRequest, HomeQueryResponse>(
            new HomeQueryRequest(),
            CancellationToken.None);

        if (response.SuccessWithValue)
        {
            Home = response.Value;
            Loading = false;
            Loaded = true;
        }
        else
        {
            Home = new HomeQueryValue();
            Loading = false;
            Loaded = true;
        }

        NotifyStateChanged();
    }
}
