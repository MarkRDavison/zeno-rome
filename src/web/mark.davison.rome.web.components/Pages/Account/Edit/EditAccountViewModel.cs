namespace mark.davison.rome.web.components.Pages.Account.Edit;

public class EditAccountViewModel : BaseViewModel<(Guid, EditAccountFormViewModel?)>
{
    private Guid _accountTypeId;
    private Guid? _loadedAccountId;
    private readonly IStartupState _startupState;
    private readonly IAccountState _accountState;
    private readonly IFormSubmission<EditAccountFormViewModel> _formSubmission;
    private readonly IClientNavigationManager _clientNavigationManager;
    private readonly IAppSessionBootstrap _appSessionBootstrap;
    private bool _isCreateNew;
    private bool _initializing;

    public EditAccountViewModel(
        IStartupState startupState,
        IAccountState accountState,
        IFormSubmission<EditAccountFormViewModel> formSubmission,
        IClientNavigationManager clientNavigationManager,
        IAppContextService appContextService,
        IAppSessionBootstrap appSessionBootstrap
    ) : base(
        appContextService)
    {
        _startupState = startupState;
        _accountState = accountState;
        _formSubmission = formSubmission;
        _clientNavigationManager = clientNavigationManager;
        _appSessionBootstrap = appSessionBootstrap;

        RegisterState(_startupState);
        RegisterState(_accountState);
    }

    public async Task<bool> InitializeForEdit(Guid accountId)
    {
        if (accountId == _loadedAccountId && FormViewModel is not null)
        {
            return false;
        }

        _loadedAccountId = accountId;
        _isCreateNew = false;
        _initializing = true;
        FormViewModel = null!;
        NotifyPropertyChanged(nameof(Loading));

        await _appSessionBootstrap.EnsureAuthenticatedStartupAsync();
        await _accountState.FetchAccount(accountId);

        var account = _accountState.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            _initializing = false;
            _clientNavigationManager.NavigateTo(Routes.NotFound);
            return true;
        }

        FormViewModel = MapAccountToForm(account);

        EditContext?.OnFieldChanged -= FieldChanged;
        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged += FieldChanged;

        _initializing = false;
        return true;
    }

    // TODO: Handle null account type id, just have to select it
    public override async Task<bool> Initialize((Guid, EditAccountFormViewModel?) payload)
    {
        await Task.CompletedTask;

        if (_accountTypeId == payload.Item1)
        {
            return false;
        }

        _accountTypeId = payload.Item1;
        _loadedAccountId = null;

        // TODO: Factory/from DI?
        _isCreateNew = payload.Item2 is null;
        FormViewModel = payload.Item2 ?? new EditAccountFormViewModel(_startupState)
        {
            AccountTypeId = _accountTypeId,
            HideAccountType = true
        };

        EditContext?.OnFieldChanged -= FieldChanged;
        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged += FieldChanged;

        return true;
    }

    public async Task OnSave()
    {
        InProgress = true;

        if (FormViewModel.Valid)
        {
            if (await _formSubmission.Primary(FormViewModel) is { Success: true })
            {
                _clientNavigationManager.NavigateTo(RouteHelpers.Account(FormViewModel.Id));
            }
        }

        InProgress = false;
    }

    internal static EditAccountFormViewModel MapAccountToForm(AccountDto account, IStartupState startupState)
    {
        var form = new EditAccountFormViewModel(startupState)
        {
            Id = account.Id,
            Name = account.Name,
            AccountNumber = account.AccountNumber,
            AccountTypeId = account.AccountTypeId,
            CurrencyId = account.CurrencyId,
            HideAccountType = true,
            VirtualBalance = account.VirtualBalance is null
                ? null
                : CurrencyRules.FromPersisted(account.VirtualBalance.Value),
            OpeningBalance = account.OpeningBalance is null
                ? null
                : CurrencyRules.FromPersisted(account.OpeningBalance.Value),
            OpeningBalanceDate = account.OpeningBalanceDate?.ToDateTime(TimeOnly.MinValue),
            IsFavourite = account.IsFavourite
        };

        return form;
    }

    private EditAccountFormViewModel MapAccountToForm(AccountDto account) =>
        MapAccountToForm(account, _startupState);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            EditContext?.OnFieldChanged -= FieldChanged;
        }

        base.Dispose(disposing);
    }

    public string Title => _isCreateNew ? "Create account" : "Edit account";
    public bool Loading => _initializing || FormViewModel is null || _accountState.Loading || !_startupState.ReferenceDataLoaded;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;
    public EditContext? EditContext { get; private set; }
    public required EditAccountFormViewModel FormViewModel { get; set; }

    private void FieldChanged(object? sender, FieldChangedEventArgs args) => this.State_StateChanged(this, EventArgs.Empty);
}
