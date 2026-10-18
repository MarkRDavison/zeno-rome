namespace mark.davison.rome.web.components.Pages.Account.View;

public class ViewAccountViewModel : BaseViewModel<Guid>
{
    private Guid _accountId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;
    private readonly ITransactionState _transactionState;
    private readonly ICategoryState _categoryState;
    private readonly IAppContextService _appContextService;
    private readonly IClientNavigationManager _clientNavigationManager;
    private readonly IAppSessionBootstrap _appSessionBootstrap;

    public ViewAccountViewModel(
        IAccountState accountState,
        IStartupState startupState,
        ITransactionState transactionState,
        ICategoryState categoryState,
        IAppContextService appContextService,
        IClientNavigationManager clientNavigationManager,
        IAppSessionBootstrap appSessionBootstrap
    ) : base(
        appContextService)
    {
        _accountState = accountState;
        _startupState = startupState;
        _transactionState = transactionState;
        _categoryState = categoryState;
        _appContextService = appContextService;
        _clientNavigationManager = clientNavigationManager;
        _appSessionBootstrap = appSessionBootstrap;

        RegisterState(_accountState);
        RegisterState(_startupState);
        RegisterState(_transactionState);
        RegisterState(_categoryState);
    }

    public override async Task<bool> Initialize(Guid payload)
    {
        _accountId = payload;

        await LoadAccountState();

        return true;
    }

    protected override async Task OnAppContextUpdated(AppContextState state)
    {
        await LoadAccountState();
    }

    protected async Task LoadAccountState()
    {
        await _appSessionBootstrap.EnsureAuthenticatedStartupAsync();

        await Task.WhenAll(
            _accountState.FetchAccount(_accountId),
            _transactionState.FetchTransactionsForAccount(_accountId),
            _categoryState.FetchState());

        if (_accountState.Loaded && Account is null)
        {
            _clientNavigationManager.NavigateTo(Routes.NotFound);
            return;
        }

        await EnsurePeerAccountsLoadedAsync();
    }

    private async Task EnsurePeerAccountsLoadedAsync()
    {
        var knownIds = _accountState.Accounts.Select(_ => _.Id).ToHashSet();
        var needsFullList = _transactionState.Transactions
            .Any(t => !knownIds.Contains(t.AccountId));

        if (needsFullList)
        {
            await _accountState.FetchState(null);
        }
    }

    public string GetFormattedAmount(Guid? currencyId, long? amount)
    {
        if (currencyId is null || amount is null)
        {
            return string.Empty;
        }

        var currency = _startupState.Currencies.FirstOrDefault(_ => _.Id == currencyId);
        if (currency is null)
        {
            return string.Empty;
        }

        return CurrencyRules.FromPersistedToFormatted(amount.Value, currency.Symbol, currency.DecimalPlaces);
    }

    public IEnumerable<ViewAccountGridRow> GenerateRows()
    {
        List<ViewAccountGridRow> items = [];
        var registerIndex = 0;

        var groups = _transactionState.Transactions
            .GroupBy(_ => _.TransactionGroupId)
            .Where(g => g.Any(t => t.AccountId == _accountId))
            .OrderByDescending(g => g.Where(t => t.AccountId == _accountId).Max(t => t.Date))
            .ThenByDescending(g => g.Key);

        foreach (var tGroup in groups)
        {
            var splitDescription = tGroup
                .Select(t => t.SplitTransactionDescription)
                .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
            var transactionsByJournal = tGroup
                .GroupBy(_ => _.TransactionJournalId)
                .Where(g => g.Any(t => t.AccountId == _accountId))
                .OrderBy(g => g.First().JournalOrder);

            if (string.IsNullOrEmpty(splitDescription) && transactionsByJournal.Count() > 1)
            {
                throw new InvalidDataException("No split description but more than 1 journals worth of transactions");
            }

            bool isSplit = !string.IsNullOrEmpty(splitDescription);
            var groupDate = tGroup.Where(t => t.AccountId == _accountId).Max(t => t.Date);

            if (isSplit)
            {
                items.Add(new ViewAccountGridRow
                {
                    RegisterIndex = registerIndex++,
                    IsSplit = true,
                    Date = groupDate,
                    Description = new()
                    {
                        Href = RouteHelpers.Transaction(tGroup.Key),
                        Text = splitDescription
                    },
                    Amount = CurrencyRules.FromPersisted(transactionsByJournal
                        .Sum(j => j.First(t => t.AccountId == _accountId).Amount))
                });
            }

            foreach (var tbjs in transactionsByJournal)
            {
                if (tbjs.Count() != 2)
                {
                    throw new InvalidDataException("Not 2 transactions under a journal");
                }

                var sourceTransaction = tbjs.First(_ => _.Source);
                var destinationTransaction = tbjs.First(_ => !_.Source);

                var sourceAccount = _accountState.Accounts.FirstOrDefault(_ => _.Id == sourceTransaction.AccountId);
                var destAccount = _accountState.Accounts.FirstOrDefault(_ => _.Id == destinationTransaction.AccountId);

                var thisAccountTransaction = tbjs.First(t => t.AccountId == _accountId);

                var transactionType = _startupState.TransactionTypes.First(_ => _.Id == thisAccountTransaction.TransactionTypeId);

                var sourceAccountLinkInfo = new LinkDefinition
                {
                    Text = sourceAccount?.Name ?? BuiltinAccountNames.GetBuiltinAccountName(sourceTransaction.AccountId),
                    Href = sourceAccount == null ? string.Empty : RouteHelpers.Account(sourceAccount.Id)
                };

                var destAccountLinkInfo = new LinkDefinition
                {
                    Text = destAccount?.Name ?? BuiltinAccountNames.GetBuiltinAccountName(destinationTransaction.AccountId),
                    Href = destAccount == null ? string.Empty : RouteHelpers.Account(destAccount.Id)
                };

                var category = thisAccountTransaction.CategoryId == null
                    ? null
                    : _categoryState.Categories.FirstOrDefault(c => c.Id == thisAccountTransaction.CategoryId);

                var categoryLinkInfo = new LinkDefinition
                {
                    Text = category?.Name ?? string.Empty,
                    Href = category == null ? string.Empty : RouteHelpers.Category(category.Id)
                };

                items.Add(new ViewAccountGridRow
                {
                    RegisterIndex = registerIndex++,
                    IsSplit = false,
                    IsSubTransaction = isSplit,
                    Description = new()
                    {
                        Text = thisAccountTransaction.Description,
                        Href = RouteHelpers.Transaction(tGroup.Key)
                    },
                    Amount = CurrencyRules.FromPersisted(thisAccountTransaction.Amount),
                    Date = thisAccountTransaction.Date,
                    TransactionGroupId = tGroup.Key,
                    TransactionType = transactionType.Type,
                    TransactionTypeId = thisAccountTransaction.TransactionTypeId,
                    Category = categoryLinkInfo,
                    SourceAccount = sourceAccountLinkInfo,
                    DestinationAccount = destAccountLinkInfo
                });
            }
        }

        return items;
    }

    public Func<ViewAccountGridRow, string> AmountCellStyle => row =>
    {
        string style = "";

        if (row.TransactionTypeId == TransactionTypeConstants.Transfer)
        {
            style += "color: #47b2f5; ";
        }
        else if (row.Amount != null)
        {
            if (row.Amount < 0.0M)
            {
                style += "color: #e47365; ";
            }
            else
            {
                style += "color: #00ad5d; ";
            }
        }

        return style;
    };

    public string RowClassFunc(ViewAccountGridRow row, int index)
    {
        return " border-style: none;";
    }

    public string BalanceAsOfPeriodEndTooltip =>
        $"Balance as of {_appContextService.State.RangeEnd.ToOrdinalShortDate()}";

    public AccountDto? Account => _accountState.Accounts.FirstOrDefault(_ => _.Id == _accountId);
    public bool Loading =>
        !_startupState.ReferenceDataLoaded ||
        (_accountId != Guid.Empty && Account is null && _accountState.Loading) ||
        IsStateLoading;
    public string Title => Account?.Name ?? string.Empty;
    public List<CommandMenuItem> CommandMenuItems { get; set; } =
    [
        new CommandMenuItem{ Text = "Edit", Id = "EDIT" },
        new CommandMenuItem{ Text = "Delete", Id = "DELETE" }
    ];

    public void OnCommandMenuItemSelected(CommandMenuItem menuItem)
    {
        if (menuItem.Id == "EDIT")
        {
            _clientNavigationManager.NavigateTo(RouteHelpers.AccountEdit(_accountId));
        }
    }
}
