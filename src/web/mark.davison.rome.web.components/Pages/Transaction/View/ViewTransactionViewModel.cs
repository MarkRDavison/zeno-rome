namespace mark.davison.rome.web.components.Pages.Transaction.View;

public class ViewTransactionViewModel : BaseViewModel<Guid>
{
    private bool _initialized;
    private Guid _transactionGroupId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;
    private readonly ICategoryState _categoryState;
    private readonly ITransactionState _transactionState;
    private readonly IAppSessionBootstrap _appSessionBootstrap;
    private readonly IClientNavigationManager _clientNavigationManager;

    public ViewTransactionViewModel(
        IAccountState accountState,
        IStartupState startupState,
        ICategoryState categoryState,
        ITransactionState transactionState,
        IAppContextService appContextService,
        IAppSessionBootstrap appSessionBootstrap,
        IClientNavigationManager clientNavigationManager
    ) : base(
        appContextService)
    {
        _accountState = accountState;
        _startupState = startupState;
        _categoryState = categoryState;
        _transactionState = transactionState;
        _appSessionBootstrap = appSessionBootstrap;
        _clientNavigationManager = clientNavigationManager;

        RegisterState(_accountState);
        RegisterState(_startupState);
        RegisterState(_categoryState);
        RegisterState(_transactionState);
    }

    public override async Task<bool> Initialize(Guid payload)
    {
        _initialized = false;
        _transactionGroupId = payload;

        await _appSessionBootstrap.EnsureAuthenticatedStartupAsync();

        await Task.WhenAll(
            _transactionState.FetchTransactions(_transactionGroupId),
            _accountState.FetchState(null),
            _categoryState.FetchState());

        if (_transactionState.Loaded &&
            !_transactionState.Transactions.Any(t => t.TransactionGroupId == _transactionGroupId))
        {
            _clientNavigationManager.NavigateTo(Routes.NotFound);
            return true;
        }

        _initialized = true;
        return true;
    }

    public IEnumerable<ViewTransactionItem> Items => ToCardItems();
    public DateOnly? TransactionDate { get; private set; }
    public string? TransactionDescription { get; private set; }
    public string? TransactionType { get; private set; }
    public string? TotalAmount { get; private set; }
    public string? TotalAmountStyle { get; private set; }
    public List<LinkDefinition> SourceAccounts { get; private set; } = [];

    public bool Loading =>
        !_startupState.ReferenceDataLoaded ||
        !_initialized ||
        IsStateLoading ||
        _transactionGroupId == Guid.Empty;

    // TODO: To helper
    private TransactionDto? GetSourceTransaction(Guid transactionTypeId, List<TransactionDto> transactions)
    {
        if (transactionTypeId == TransactionTypeConstants.Transfer)
        {
            return transactions.FirstOrDefault(_ => _.Source);
        }
        else if (
            transactionTypeId == TransactionTypeConstants.Deposit ||
            transactionTypeId == TransactionTypeConstants.Withdrawal
        )
        {
            var sourceTransactionAccountTypes = AllowableSourceDestinationAccounts.GetSourceAccountTypes(transactionTypeId);

            return transactions.FirstOrDefault(_ =>
            {
                var account = _accountState.Accounts.FirstOrDefault(__ => __.Id == _.AccountId);
                return account is not null && sourceTransactionAccountTypes.Contains(account.AccountTypeId);
            });
        }

        return transactions.FirstOrDefault(_ => _.Source);
    }

    // TODO: To helper
    private TransactionDto? GetDestinationTransaction(Guid transactionTypeId, Guid sourceAccountId, Guid sourceAccountTypeId, List<TransactionDto> transactions)
    {
        if (transactionTypeId == TransactionTypeConstants.Transfer)
        {
            return transactions.FirstOrDefault(_ => !_.Source);
        }
        else if (
            transactionTypeId == TransactionTypeConstants.Deposit ||
            transactionTypeId == TransactionTypeConstants.Withdrawal
        )
        {
            var destTransactionAccountTypes = AllowableSourceDestinationAccounts.GetDestinationAccountTypesForSource(transactionTypeId, sourceAccountTypeId);

            return transactions.FirstOrDefault(_ =>
            {
                if (_.AccountId == sourceAccountId)
                {
                    return false;
                }

                var account = _accountState.Accounts.FirstOrDefault(__ => __.Id == _.AccountId);
                return account is not null && destTransactionAccountTypes.Contains(account.AccountTypeId);
            });
        }

        return transactions.FirstOrDefault(_ => !_.Source);
    }

    // TODO: Re-use/helperise, ViewAccount.razor.cs
    private string GetAmountText(Guid transactionTypeId, long amount, CurrencyDto? currency)
    {
        if (transactionTypeId == TransactionTypeConstants.Transfer)
        {
            amount = Math.Abs(amount);
        }
        else if (transactionTypeId == TransactionTypeConstants.Deposit)
        {
            amount = Math.Abs(amount);
        }

        bool negative = amount < 0;

        amount = Math.Abs(amount);

        return $"{(negative ? "-" : string.Empty)}{currency?.Symbol}{CurrencyRules.FromPersisted(amount).ToString($"N{currency?.DecimalPlaces ?? 2}")}";
    }

    // TODO: REPLACE WITH SCSS class
    private static string GetAmountColour(Guid transactionTypeId, long amount)
    {
        if (transactionTypeId == TransactionTypeConstants.Transfer)
        {
            return "color: #47b2f5; "; // blue
        }

        if (transactionTypeId == TransactionTypeConstants.Deposit)
        {
            if (amount < 0)
            {
                return "color: #00ad5d; "; // green
            }
            else
            {
                return "color: #e47365; "; // red
            }
        }

        if (transactionTypeId == TransactionTypeConstants.Withdrawal)
        {
            if (amount < 0)
            {
                return "color: #e47365; "; // red
            }
            else
            {
                return "color: #00ad5d; "; // green
            }
        }

        if (amount < 0)
        {
            return "color: #e47365; "; // red
        }
        else
        {
            return "color: #00ad5d; "; // green
        }
    }

    private IEnumerable<ViewTransactionItem> ToCardItems()
    {
        CurrencyDto? currency = null;
        Guid transactionTypeId = Guid.Empty;
        SourceAccounts.Clear();

        var items = _transactionState.Transactions
            .Where(_ => _.TransactionGroupId == _transactionGroupId)
            .GroupBy(_ => _.TransactionJournalId)
            .Select(_ =>
            {
                // TODO: NEXT: CURRENT: MEPLEASE:
                //  
                // Source and dest are not determined for this display using the Source property
                //
                // Instead it is based on the transaction type
                //  - Transfer: is a special case, uses blue and its neither a positive or a negative
                //  - Deposit: source is the revenue account, i.e. from work account -> (GREEN) -> saving account
                //  - Withdrawal: source is the asset account, i.e. from checking -> (RED) -> supermarket account
                //
                // Need to make a utility for getting the styles on the numbers, css/c#???
                var transactions = _.ToList();

                transactionTypeId = transactions.Select(__ => __.TransactionTypeId).First();

                var source = GetSourceTransaction(transactionTypeId, transactions);

                if (source is null)
                {
                    return (ViewTransactionItem?)null;
                }

                var sourceAccount = _accountState.Accounts.FirstOrDefault(__ => __.Id == source.AccountId);

                if (sourceAccount == null)
                {
                    return (ViewTransactionItem?)null;
                }

                var dest = GetDestinationTransaction(transactionTypeId, source.AccountId, sourceAccount.AccountTypeId, transactions);

                if (dest is null)
                {
                    return (ViewTransactionItem?)null;
                }

                var destAccount = _accountState.Accounts.FirstOrDefault(__ => __.Id == dest.AccountId);

                if (destAccount == null)
                {
                    return (ViewTransactionItem?)null;
                }

                currency = _startupState.Currencies.FirstOrDefault(__ => __.Id == sourceAccount.CurrencyId);

                if (currency == null)
                {
                    return (ViewTransactionItem?)null;
                }

                var category = _categoryState.Categories.FirstOrDefault(__ => __.Id == source.CategoryId);

                var transactionType = _startupState.TransactionTypes.First(__ => __.Id == transactionTypeId);

                TransactionType = transactionType.Type;
                TransactionDescription = _.First().SplitTransactionDescription;
                TransactionDate = _.First().Date;

                return new ViewTransactionItem
                {
                    Description = source.Description,
                    SourceAccount = new LinkDefinition
                    {
                        Text = sourceAccount.Name,
                        Href = RouteHelpers.Account(sourceAccount.Id)
                    },
                    DestinationAccount = new LinkDefinition
                    {
                        Text = destAccount.Name,
                        Href = RouteHelpers.Account(destAccount.Id)
                    },
                    Amount = GetAmountText(transactionTypeId, source.Amount, currency),
                    AmountValue = source.Amount,
                    ForeignAmount = "",
                    Category = category == null ? null : new LinkDefinition
                    {
                        Text = category.Name,
                        Href = RouteHelpers.Category(category.Id)
                    },
                    Tags = source.Tags,
                    AmountStyle = GetAmountColour(transactionTypeId, source.Amount)
                };
            })
            .OfType<ViewTransactionItem>()
            .ToList();

        var totalAmount = items.Sum(_ => _.AmountValue);
        TotalAmount = GetAmountText(transactionTypeId, totalAmount, currency);
        TotalAmountStyle = GetAmountColour(transactionTypeId, totalAmount);
        SourceAccounts.AddRange(items.Select(_ => _.SourceAccount).DistinctBy(_ => _.Href));

        return items;
    }
}
