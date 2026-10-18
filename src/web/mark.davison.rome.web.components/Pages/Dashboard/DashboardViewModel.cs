namespace mark.davison.rome.web.components.Pages.Dashboard;

public sealed class DashboardViewModel : BaseViewModel<object?>
{
    private bool _isLoading = true;
    private readonly IStartupState _startupState;
    private readonly IHomeState _homeState;
    private readonly IAppSessionBootstrap _appSessionBootstrap;

    public DashboardViewModel(
        IStartupState startupState,
        IHomeState homeState,
        IAppContextService appContextService,
        IAppSessionBootstrap appSessionBootstrap) : base(appContextService)
    {
        _startupState = startupState;
        _homeState = homeState;
        _appSessionBootstrap = appSessionBootstrap;

        RegisterState(_startupState);
        RegisterState(_homeState);
    }

    public override async Task<bool> Initialize(object? payload)
    {
        _isLoading = true;
        await _appSessionBootstrap.EnsureAuthenticatedStartupAsync();
        await _homeState.FetchState();
        _isLoading = false;
        return true;
    }

    protected override async Task OnAppContextUpdated(AppContextState state)
    {
        if (!_isLoading)
        {
            await _homeState.FetchState();
        }
    }

    public bool Loading =>
        _isLoading ||
        !_startupState.BootstrapComplete ||
        _homeState.Loading ||
        IsStateLoading;

    public IEnumerable<DashboardAccountItemViewModel> Accounts =>
        (_homeState.Home?.Accounts ?? [])
            .Select(CreateAccountItem);

    public IEnumerable<DashboardRecentItemViewModel> RecentTransactions =>
        (_homeState.Home?.RecentTransactions ?? [])
            .Select(CreateRecentItem);

    private DashboardAccountItemViewModel CreateAccountItem(AccountDto account)
    {
        var currency = _startupState.Currencies.First(_ => _.Id == account.CurrencyId);
        return new DashboardAccountItemViewModel
        {
            Id = account.Id,
            Name = account.Name,
            AccountType = _startupState.AccountTypes.First(_ => _.Id == account.AccountTypeId).Type,
            CurrentBalance = CurrencyRules.FromPersistedToFormatted(account.CurrentBalance, currency.Symbol, currency.DecimalPlaces),
            BalanceDifference = CurrencyRules.FromPersistedToFormatted(account.BalanceDifference, currency.Symbol, currency.DecimalPlaces),
            BalanceDifferenceAmount = account.BalanceDifference,
            AccountHref = RouteHelpers.Account(account.Id)
        };
    }

    private DashboardRecentItemViewModel CreateRecentItem(HomeRecentTransactionDto recent)
    {
        var transactionType = recent.TransactionTypeId is null
            ? string.Empty
            : _startupState.TransactionTypes.FirstOrDefault(_ => _.Id == recent.TransactionTypeId)?.Type ?? string.Empty;

        return new DashboardRecentItemViewModel
        {
            TransactionGroupId = recent.TransactionGroupId,
            Title = string.IsNullOrWhiteSpace(recent.Title) ? "Transaction" : recent.Title,
            TransactionType = transactionType,
            Date = recent.Date,
            Href = RouteHelpers.Transaction(recent.TransactionGroupId)
        };
    }

    internal Func<DashboardAccountItemViewModel, string> AmountCellStyleFunc(Func<DashboardAccountItemViewModel, long> amountSelector) => item =>
    {
        var amount = amountSelector(item);
        // TODO: Classes etc instead of hardcoded colours.  
        // TODO: MORE CONSISTENCY.
        return amount < 0 ? "color: #e47365; " : "color: #00ad5d; ";
    };
}

public sealed class DashboardAccountItemViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public string CurrentBalance { get; init; } = string.Empty;
    public string BalanceDifference { get; init; } = string.Empty;
    public long BalanceDifferenceAmount { get; init; }
    public string AccountHref { get; init; } = string.Empty;
}

public sealed class DashboardRecentItemViewModel
{
    public Guid TransactionGroupId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string TransactionType { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public string Href { get; init; } = string.Empty;
}
