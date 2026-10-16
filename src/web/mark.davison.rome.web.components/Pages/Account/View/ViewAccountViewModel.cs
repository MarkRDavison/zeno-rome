namespace mark.davison.rome.web.components.Pages.Account.View;

public class ViewAccountViewModel : BaseViewModel<Guid>
{
    private Guid _accountId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;
    private readonly ITransactionState _transactionState;
    private readonly ICategoryState _categoryState;
    private readonly IAppContextService _appContextService;

    public ViewAccountViewModel(
        IAccountState accountState,
        IStartupState startupState,
        ITransactionState transactionState,
        ICategoryState categoryState,
        IAppContextService appContextService
    ) : base(
        appContextService)
    {
        _accountState = accountState;
        _startupState = startupState;
        _transactionState = transactionState;
        _categoryState = categoryState;
        _appContextService = appContextService;

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
        await _accountState.FetchAccount(_accountId);
        await _transactionState.FetchTransactionsForAccount(_accountId);
        await _categoryState.FetchState();
    }

    public string GetFormattedAmount(Guid? currencyId, long? amount)
    {
        if (currencyId is null || amount is null)
        {
            return string.Empty;
        }

        var currency = _startupState.Currencies.First(_ => _.Id == currencyId);

        return CurrencyRules.FromPersistedToFormatted(amount.Value, currency.Symbol, currency.DecimalPlaces);
    }

    public IEnumerable<ViewAccountGridRow> GenerateRows()
    {
        var rangeStart = _appContextService.State.RangeStart;
        var rangeEnd = _appContextService.State.RangeEnd;
        List<ViewAccountGridRow> items = [];
        var registerIndex = 0;

        var groups = _transactionState.Transactions
            .GroupBy(_ => _.TransactionGroupId)
            .Where(g => g.Any(t =>
                t.AccountId == _accountId &&
                rangeStart <= t.Date &&
                t.Date <= rangeEnd))
            .OrderByDescending(g => g.Where(t => t.AccountId == _accountId).Max(t => t.Date))
            .ThenByDescending(g => g.Key);

        foreach (var tGroup in groups)
        {
            var splitDescription = tGroup.First().SplitTransactionDescription ?? string.Empty;
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

    public AccountDto? Account => _accountState.Accounts.FirstOrDefault(_ => _.Id == _accountId);
    public bool Loading => _accountId != Guid.Empty && Account is null || IsStateLoading;
    public string Title => Account?.Name ?? string.Empty;
    public List<CommandMenuItem> CommandMenuItems { get; set; } =
    [
        new CommandMenuItem{ Text = "Edit", Id = "EDIT" },
        new CommandMenuItem{ Text = "Delete", Id = "DELETE" }
    ];
}
