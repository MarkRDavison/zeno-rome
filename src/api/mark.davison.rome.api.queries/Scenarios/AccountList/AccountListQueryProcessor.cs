namespace mark.davison.rome.api.queries.Scenarios.AccountList;

public sealed class AccountListQueryProcessor : IQueryProcessor<AccountListQueryRequest, AccountListQueryResponse>
{
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
               _.UserId == currentUserContext.UserId &&
               (request.AccountType == null || _.AccountTypeId == request.AccountType) &&
               AccountQuerySupport.IsUserVisibleAccount(_.Id))
           .ToListAsync(cancellationToken);

        var accountIds = accounts.Select(_ => _.Id).ToList();

        var openingBalances = await AccountQuerySupport.LoadOpeningBalanceJournalsAsync(
            _dbContext,
            accountIds,
            cancellationToken);

        foreach (var account in accounts)
        {
            var openingBalanceTransactionJournal = openingBalances
                .FirstOrDefault(_ => _.Transactions
                    .Any(t => t.AccountId == account.Id));

            var openingBalanceTransaction = openingBalanceTransactionJournal?
                .Transactions
                .FirstOrDefault(_ => _.AccountId == account.Id);

            var amounts = await AccountQuerySupport.LoadTransactionAmountsAsync(
                _dbContext,
                account.Id,
                cancellationToken);

            response.Value.Add(AccountQuerySupport.ToAccountDto(
                account,
                openingBalanceTransactionJournal,
                openingBalanceTransaction,
                amounts,
                _financeUserContext));
        }

        return response;
    }
}
