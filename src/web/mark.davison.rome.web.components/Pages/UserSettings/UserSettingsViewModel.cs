namespace mark.davison.rome.web.components.Pages.UserSettings;

public sealed class UserSettingsViewModel : BaseViewModel<object?>
{
    private readonly IClientHttpRepository _clientHttpRepository;
    private readonly ISnackbar _snackbar;

    public UserSettingsViewModel(
        IClientHttpRepository clientHttpRepository,
        ISnackbar snackbar,
        IAppContextService appContextService) : base(appContextService)
    {
        _clientHttpRepository = clientHttpRepository;
        _snackbar = snackbar;
    }

    public int HomeRecentTransactionCountMin => UserPreferenceConstants.HomeRecentTransactionCountMin;
    public int HomeRecentTransactionCountMax => UserPreferenceConstants.HomeRecentTransactionCountMax;

    public int HomeRecentTransactionCount { get; set; }
    public bool Loading { get; private set; }
    public bool Saving { get; private set; }

    public override async Task<bool> Initialize(object? payload)
    {
        Loading = true;
        NotifyPropertyChanged(nameof(Loading));

        var response = await _clientHttpRepository.Get<GetUserPreferencesQueryRequest, GetUserPreferencesQueryResponse>(
            new GetUserPreferencesQueryRequest(),
            CancellationToken.None);

        if (response.SuccessWithValue)
        {
            HomeRecentTransactionCount = response.Value.HomeRecentTransactionCount;
        }
        else
        {
            HomeRecentTransactionCount = UserPreferenceConstants.HomeRecentTransactionCountDefault;
        }

        Loading = false;
        NotifyPropertyChanged(nameof(Loading));
        NotifyPropertyChanged(nameof(HomeRecentTransactionCount));
        return true;
    }

    public async Task SaveAsync()
    {
        Saving = true;
        NotifyPropertyChanged(nameof(Saving));

        var request = new UpsertUserPreferencesCommandRequest
        {
            Preferences = new UserPreferencesDto { HomeRecentTransactionCount = HomeRecentTransactionCount }
        };

        var response = await _clientHttpRepository.Post<UpsertUserPreferencesCommandRequest, UpsertUserPreferencesCommandResponse>(
            request,
            CancellationToken.None);

        if (response.SuccessWithValue)
        {
            HomeRecentTransactionCount = response.Value.HomeRecentTransactionCount;
            _snackbar.Add("Settings saved.", Severity.Success);
        }
        else
        {
            _snackbar.Add("Could not save settings.", Severity.Error);
        }

        Saving = false;
        NotifyPropertyChanged(nameof(Saving));
        NotifyPropertyChanged(nameof(HomeRecentTransactionCount));
    }
}
