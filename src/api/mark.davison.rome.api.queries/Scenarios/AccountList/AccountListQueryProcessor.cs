namespace mark.davison.rome.api.queries.Scenarios.AccountList;

public sealed class AccountListQueryProcessor : IQueryProcessor<AccountListQueryRequest, AccountListQueryResponse>
{
    private class DatedTransactionAmount
    {
        public required long Amount { get; init; }
        public required DateOnly Date { get; init; }
    }

    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IFinanceUserContext _financeUserContext;

    public AccountListQueryProcessor(
        IDbContext<RomeDbContext> dbContext,
        IFinanceUserContext financeUserContext)
    {
        _dbContext = dbContext;
        _financeUserContext = financeUserContext;
    }

    public async Task<AccountListQueryResponse> ProcessAsync(AccountListQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var response = new AccountListQueryResponse
        {
            Value = []
        };

        var accounts = await _dbContext
           .Set<Account>()
           .AsNoTracking()
           .Include(_ => _.AccountType)
           .Where(_ =>
               _.UserId == currentUserContext.UserId && // TODO: Do I need anything other than the user query???
               (request.AccountType == null || _.AccountTypeId == request.AccountType) &&
               _.Id != AccountConstants.Reconciliation && // TODO: Better single place that creates expression to filter these or add property to account
               _.Id != AccountConstants.OpeningBalance)
           .ToListAsync(cancellationToken);

        var accountIds = accounts.Select(_ => _.Id).ToList();

        var openingBalances = await _dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .Where(_ =>
                _.Transactions.Any(__ => accountIds.Contains(__.AccountId)) &&
                _.TransactionTypeId == TransactionTypeConstants.OpeningBalance)
            .ToListAsync(cancellationToken);

        foreach (var account in accounts)
        {
            var openingBalanceTransactionJournal = openingBalances
                .FirstOrDefault(_ => _.Transactions
                    .Any(t => t.AccountId == account.Id));

            var openingBalanceTransaction = openingBalanceTransactionJournal?
                .Transactions
                .FirstOrDefault(_ => _.AccountId == account.Id);

            // TODO: Long term need a cache for this?
            var amounts = await _dbContext
                .Set<Transaction>()
                .AsNoTracking()
                .Where(_ => _.AccountId == account.Id)
                .Select(_ => new DatedTransactionAmount { Amount = _.Amount, Date = _.TransactionJournal!.Date })
                .ToListAsync(cancellationToken);

            var currentBalance = amounts
                .Where(_ => _.Date <= _financeUserContext.RangeEnd)
                .Sum(_ => _.Amount);

            var balanceDifference = amounts
                .Where(_ => _financeUserContext.RangeStart <= _.Date && _.Date <= _financeUserContext.RangeEnd)
                .Sum(_ => _.Amount);

            response.Value.Add(new AccountDto(
                account.Id,
                account.Name,
                account.AccountTypeId,
                account.AccountNumber,
                currentBalance,
                account.IsActive,
                account.LastModified,
                balanceDifference,
                account.CurrencyId,
                account.VirtualBalance,
                openingBalanceTransaction?.Amount,
                openingBalanceTransactionJournal?.Date));
        }

        return response;
    }
}
