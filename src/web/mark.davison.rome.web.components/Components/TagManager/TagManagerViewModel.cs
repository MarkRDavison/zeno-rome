namespace mark.davison.rome.web.components.Components.TagManager;

public sealed class TagManagerViewModel : BaseViewModel<object?>
{
    private readonly ITagState _tagState;
    private readonly IClientHttpRepository _clientHttpRepository;
    private Guid? _editingTagId;
    private string _editingName = string.Empty;
    private string _newTagName = string.Empty;
    private string? _errorMessage;
    private bool _busy;

    public TagManagerViewModel(
        IAppContextService appContextService,
        ITagState tagState,
        IClientHttpRepository clientHttpRepository) : base(appContextService)
    {
        _tagState = tagState;
        _clientHttpRepository = clientHttpRepository;
        RegisterState(_tagState);
    }

    public IEnumerable<TagDto> Tags => _tagState.Tags;

    public bool Loading => _tagState.Loading;

    public string NewTagName
    {
        get => _newTagName;
        set
        {
            _newTagName = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(CanAddNew));
        }
    }

    public Guid? EditingTagId => _editingTagId;

    public string EditingName
    {
        get => _editingName;
        set
        {
            _editingName = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(CanSaveEdit));
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            _errorMessage = value;
            NotifyPropertyChanged();
        }
    }

    public bool Busy
    {
        get => _busy;
        private set
        {
            _busy = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(CanAddNew));
            NotifyPropertyChanged(nameof(CanSaveEdit));
        }
    }

    public bool CanAddNew => !Busy && !string.IsNullOrWhiteSpace(NewTagName) && !IsDuplicateName(NewTagName, null);

    public bool CanSaveEdit =>
        !Busy &&
        _editingTagId is not null &&
        !string.IsNullOrWhiteSpace(EditingName) &&
        !IsDuplicateName(EditingName, _editingTagId);

    public override async Task<bool> Initialize(object? payload)
    {
        ErrorMessage = null;
        await _tagState.FetchState();
        return true;
    }

    public void StartEdit(TagDto tag)
    {
        _editingTagId = tag.Id;
        EditingName = tag.Name;
        ErrorMessage = null;
        NotifyPropertyChanged(nameof(EditingTagId));
    }

    public void CancelEdit()
    {
        _editingTagId = null;
        EditingName = string.Empty;
        NotifyPropertyChanged(nameof(EditingTagId));
    }

    public async Task SaveNewAsync()
    {
        if (!CanAddNew)
        {
            return;
        }

        Busy = true;
        ErrorMessage = null;

        var request = new UpsertTagCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = NewTagName.Trim()
        };

        var response = await _clientHttpRepository.Post<UpsertTagCommandRequest, UpsertTagCommandResponse>(request, CancellationToken.None);

        Busy = false;

        if (response.SuccessWithValue)
        {
            NewTagName = string.Empty;
            await _tagState.FetchState();
        }
        else
        {
            ErrorMessage = response.Errors?.FirstOrDefault() ?? "Could not save tag.";
        }
    }

    public async Task SaveEditAsync()
    {
        if (!CanSaveEdit || _editingTagId is null)
        {
            return;
        }

        Busy = true;
        ErrorMessage = null;

        var request = new UpsertTagCommandRequest
        {
            Id = _editingTagId.Value,
            Name = EditingName.Trim()
        };

        var response = await _clientHttpRepository.Post<UpsertTagCommandRequest, UpsertTagCommandResponse>(request, CancellationToken.None);

        Busy = false;

        if (response.SuccessWithValue)
        {
            CancelEdit();
            await _tagState.FetchState();
        }
        else
        {
            ErrorMessage = response.Errors?.FirstOrDefault() ?? "Could not save tag.";
        }
    }

    private bool IsDuplicateName(string name, Guid? excludeId)
    {
        return _tagState.Tags.Any(t =>
            t.Name.Equals(name.Trim(), StringComparison.Ordinal) &&
            t.Id != excludeId);
    }

}
