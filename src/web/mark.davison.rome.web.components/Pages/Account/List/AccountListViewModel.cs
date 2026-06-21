namespace mark.davison.rome.web.components.Pages.Account.List;

public class AccountListViewModel : BaseViewModel<Guid?>
{
    private bool _isLoading = true;
    private Guid? _accountTypeId = Guid.Empty;
    private readonly IStartupState _startupState;
    private readonly IAccountState _accountState;
    private readonly IClientNavigationManager _clientNavigationManager;

    public AccountListViewModel(
        IStartupState startupState,
        IAccountState accountState,
        IClientNavigationManager clientNavigationManager,
        IAppContextService appContextService
    ) : base(
        appContextService)
    {
        _startupState = startupState;
        _accountState = accountState;
        _clientNavigationManager = clientNavigationManager;

        RegisterState(_startupState);
        RegisterState(_accountState);
    }

    public override async Task<bool> Initialize(Guid? payload)
    {
        if (_accountTypeId == payload)
        {
            return false;
        }

        _accountTypeId = payload;

        await _accountState.FetchState(payload);

        _isLoading = false;

        return true;
    }

    public bool Loading =>
        _isLoading ||
        !_startupState.BootstrapComplete ||
        _startupState.AccountTypes.Count is 0;

    public bool AccountsLoading => Loading || IsStateLoading;

    public string Title => Loading || _accountTypeId is null
        ? "Accounts"
        : $"{_startupState.AccountTypes.First(_ => _.Id == _accountTypeId).Type} accounts";

    internal IEnumerable<AccountListItemViewModel> Items => _accountState.Accounts.Where(AccountDisplayPredicate).Select(CreateListItemViewModel);

    private bool AccountDisplayPredicate(AccountDto dto)
    {
        return _accountTypeId is null || dto.AccountTypeId == _accountTypeId;
    }

    private AccountListItemViewModel CreateListItemViewModel(AccountDto account)
    {
        var currency = _startupState.Currencies.FirstOrDefault(_ => _.Id == account.CurrencyId);

        if (currency is null)
        {
            Console.WriteLine("Trying to find {0}", account.CurrencyId);

            foreach (var c in _startupState.Currencies)
            {
                Console.WriteLine(" - {0} ({1})", c.Name, c.Id);
            }

            throw new InvalidOperationException($"Could not find currency: {account.CurrencyId}");
        }

        return new AccountListItemViewModel
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountType = _startupState.AccountTypes.First(_ => _.Id == account.AccountTypeId).Type,
            Active = account.Active,
            BalanceDifference = CurrencyRules.FromPersistedToFormatted(account.BalanceDifference, currency.Symbol, currency.DecimalPlaces),
            BalanceDifferenceAmount = 0,
            CurrentBalance = CurrencyRules.FromPersistedToFormatted(account.CurrentBalance, currency.Symbol, currency.DecimalPlaces),
            CurrentBalanceAmount = 0,
            LastModified = account.LastModified.LocalDateTime,
            Name = new LinkDefinition { Text = account.Name, Href = RouteHelpers.Account(account.Id) },
        };
    }

    internal Func<AccountListItemViewModel, string> AmountCellStyleFunc(Func<AccountListItemViewModel, long> amountSelector) => _ =>
    {
        string style = "";

        if (amountSelector(_) < 0.0M)
        {
            // TODO: Common utility
            style += "color: #e47365; ";
        }
        else
        {
            style += "color: #00ad5d; ";
        }

        return style;
    };

    public void AddAccount()
    {
        if (_accountTypeId is not null)
        {
            _clientNavigationManager.NavigateTo(RouteHelpers.AccountNew(_accountTypeId.Value));
        }
    }
}
