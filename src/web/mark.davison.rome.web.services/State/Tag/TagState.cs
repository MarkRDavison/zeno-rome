namespace mark.davison.rome.web.services.State.Tag;

internal sealed class TagState : ITagState
{
    private readonly IClientHttpRepository _clientRepository;

    public TagState(IClientHttpRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public IList<TagDto> Tags { get; private set; } = [];
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }

    public event EventHandler StateChanged = default!;

    private void SetState(IList<TagDto> tags)
    {
        Tags = [.. tags];
        Loading = false;
        Loaded = true;
        NotifyStateChanged();
    }

    public async Task FetchState()
    {
        Tags = [];
        Loading = true;
        Loaded = false;

        NotifyStateChanged();

        var response = await _clientRepository.Get<TagListQueryRequest, TagListQueryResponse>(CancellationToken.None);

        if (response.SuccessWithValue)
        {
            SetState([.. response.Value]);
        }
        else
        {
            SetState([]);
        }
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
