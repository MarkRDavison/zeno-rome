namespace mark.davison.rome.web.services.State.Category;

internal sealed class CategoryState : ICategoryState
{
    private readonly IClientHttpRepository _clientRepository;

    public CategoryState(IClientHttpRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public IList<CategoryDto> Categories { get; private set; } = [];
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }

    public event EventHandler StateChanged = default!;

    private void SetState(IList<CategoryDto> categories)
    {
        Categories = [.. categories];
        Loading = false;
        Loaded = true;
        NotifyStateChanged();
    }

    public async Task FetchState()
    {
        Categories = [];
        Loading = true;
        Loaded = false;

        NotifyStateChanged();

        var response = await _clientRepository.Get<CategoryListQueryRequest, CategoryListQueryResponse>(CancellationToken.None);

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
