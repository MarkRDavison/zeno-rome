namespace mark.davison.rome.web.components.Pages.Account.View;

public class ViewAccountViewModel : BaseViewModel<Guid>
{
    private Guid _accountId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;
    private readonly ITransactionState _transactionState;
    private readonly IAppContextService _appContextService;

    public ViewAccountViewModel(
        IAccountState accountState,
        IStartupState startupState,
        ITransactionState transactionState,
        IAppContextService appContextService
    ) : base(
        appContextService)
    {
        _accountState = accountState;
        _startupState = startupState;
        _transactionState = transactionState;
        _appContextService = appContextService;

        RegisterState(_accountState);
        RegisterState(_startupState);
        RegisterState(_transactionState);
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
        List<ViewAccountGridRow> items = [];

        foreach (var tGroup in _transactionState.Transactions
            .GroupBy(_ => _.TransactionGroupId)
            .Where(_ => _appContextService.State.RangeStart <= _.First().Date || _appContextService.State.RangeEnd <= _.First().Date)
            .OrderByDescending(_ => _.First().Date)) // TODO: Sorting with multi line entries
        {
            var splitDescription = tGroup.First().SplitTransactionDescription ?? string.Empty;

            var transactionsByJournal = tGroup.GroupBy(_ => _.TransactionJournalId);

            // TODO: Helper method on transaction state to get data in useful format by account/category/tag etc etc
            if (!transactionsByJournal.Any(g => g.Any(t => t.AccountId == _accountId)))
            {
                continue;
            }

            if (string.IsNullOrEmpty(splitDescription) && transactionsByJournal.Count() > 1)
            {
                throw new InvalidDataException("No split description but more than 1 journals worth of transactions");
            }

            bool isSplit = !string.IsNullOrEmpty(splitDescription);

            if (!string.IsNullOrEmpty(splitDescription))
            {
                items.Add(new ViewAccountGridRow
                {
                    IsSplit = true,
                    Description = new()
                    {
                        Href = RouteHelpers.Transaction(tGroup.Key),
                        Text = splitDescription
                    },
                    Amount = CurrencyRules.FromPersisted(transactionsByJournal
                        // Dont sum splits for other accounts for the total
                        .Where(_ => _.Any(__ => __.AccountId == _accountId))
                        .Sum(_ => _.First(_ => _.Source).Amount)) // TODO: BETTER
                });
            }

            foreach (var tbjs in transactionsByJournal)
            {
                // TODO: When navigating here directly, if there are splits this account is
                // not directly involved in, they are not loaded
                if (tbjs.Count() != 2)
                {
                    throw new InvalidDataException("Not 2 transactions under a journal");
                }

                var sourceTransaction = tbjs.First(_ => _.Source);
                var destinationTransaction = tbjs.First(_ => !_.Source);

                var sourceAccount = _accountState.Accounts.FirstOrDefault(_ => _.Id == sourceTransaction.AccountId); ;
                var destAccount = _accountState.Accounts.FirstOrDefault(_ => _.Id == destinationTransaction.AccountId); ;

                var thisAccountTransaction = sourceAccount == null ? destinationTransaction : (sourceAccount.Id == _accountId ? sourceTransaction : destinationTransaction);

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

                var categoryLinkInfo = new LinkDefinition
                {
                    Text = string.Empty, //thisAccountTransaction.CategoryId == null ? string.Empty : CategoryListState.Value.Categories.FirstOrDefault(_ => _.Id == thisAccountTransaction.CategoryId)?.Name ?? string.Empty,
                    Href = string.Empty //thisAccountTransaction.CategoryId == null ? string.Empty : RouteHelpers.Category(thisAccountTransaction.CategoryId.Value)
                };

                items.Add(new ViewAccountGridRow
                {
                    IsSplit = false,
                    IsSubTransaction = isSplit,
                    Description = new()
                    {
                        Text = thisAccountTransaction.Description,
                        Href = RouteHelpers.Transaction(tGroup.Key)
                    },
                    Amount = CurrencyRules.FromPersisted(thisAccountTransaction.Amount), // TODO: Re-use from ViewTransaction.razor.cs GetAmountText? maybe helper classes etc
                    Date = thisAccountTransaction.Date,
                    TransactionGroupId = tGroup.Key,
                    TransactionType = transactionType.Type,// TODO: TransactionTypeId???
                    Category = categoryLinkInfo,
                    SourceAccount = sourceAccountLinkInfo,
                    DestinationAccount = destAccountLinkInfo
                });
            }
        }

        return items.OrderByDescending(_ => _.Date);
    }

    public Func<ViewAccountGridRow, string> AmountCellStyle => _ =>
    {
        string style = "";

        if (_.TransactionType == "Transfer") // TODO: Id???
        {
            style += "color: #47b2f5; ";
        }
        else if (_.Amount != null)
        {
            if (_.Amount < 0.0M)
            {
                // TODO: Common utility
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
        // This is the style that sets the borders
        // mud-table-cell > border-bottom: 1px solid etc...
        return " border-style: none;"; // border style based on whether it is a split, the last of a day etc
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
