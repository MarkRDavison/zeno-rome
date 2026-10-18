namespace mark.davison.rome.api.queries.Scenarios.AccountById;

public sealed class AccountByIdQueryProcessor : IQueryProcessor<AccountByIdQueryRequest, AccountByIdQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IFinanceUserContext _financeUserContext;

    public AccountByIdQueryProcessor(
        IDbContext<RomeDbContext> dbContext,
        IFinanceUserContext financeUserContext)
    {
        _dbContext = dbContext;
        _financeUserContext = financeUserContext;
    }

    public async Task<AccountByIdQueryResponse> ProcessAsync(
        AccountByIdQueryRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var response = new AccountByIdQueryResponse();

        if (!AccountQuerySupport.IsUserVisibleAccount(request.AccountId))
        {
            response.Errors.Add("ACCOUNT_NOT_FOUND");
            return response;
        }

        var account = await _dbContext
            .Set<Account>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                _ => _.Id == request.AccountId && _.UserId == currentUserContext.UserId,
                cancellationToken);

        if (account is null)
        {
            response.Errors.Add("ACCOUNT_NOT_FOUND");
            return response;
        }

        var openingBalances = await AccountQuerySupport.LoadOpeningBalanceJournalsAsync(
            _dbContext,
            [account.Id],
            cancellationToken);

        var openingBalanceTransactionJournal = openingBalances
            .FirstOrDefault(_ => _.Transactions.Any(t => t.AccountId == account.Id));

        var openingBalanceTransaction = openingBalanceTransactionJournal?
            .Transactions
            .FirstOrDefault(_ => _.AccountId == account.Id);

        var amounts = await AccountQuerySupport.LoadTransactionAmountsAsync(
            _dbContext,
            account.Id,
            cancellationToken);

        response.Value = AccountQuerySupport.ToAccountDto(
            account,
            openingBalanceTransactionJournal,
            openingBalanceTransaction,
            amounts,
            _financeUserContext);

        return response;
    }
}
