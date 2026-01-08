namespace mark.davison.rome.api.queries.Scenarios.Startup;

public sealed class StartupQueryProcessor : IQueryProcessor<StartupQueryRequest, StartupQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IFinanceUserContext _financeUserContext;

    public StartupQueryProcessor(
        IDbContext<RomeDbContext> dbContext,
        IFinanceUserContext financeUserContext)
    {
        _dbContext = dbContext;
        _financeUserContext = financeUserContext;
    }

    public async Task<StartupQueryResponse> ProcessAsync(StartupQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var accountTypes = await _dbContext
            .Set<AccountType>()
            .AsNoTracking()
            .Select(_ => new AccountTypeDto(_.Id, _.Type))
            .ToListAsync(cancellationToken);

        var currencies = await _dbContext
            .Set<Currency>()
            .AsNoTracking()
            .Where(_ => _.Id != CurrencyConstants.INT)
            .Select(_ => new CurrencyDto(_.Id, _.Code, _.Name, _.Symbol, _.DecimalPlaces))
            .ToListAsync(cancellationToken);

        var transactionTypes = await _dbContext
            .Set<TransactionType>()
            .AsNoTracking()
            .Select(_ => new TransactionTypeDto(_.Id, _.Type))
            .ToListAsync(cancellationToken);

        var response = new StartupQueryResponse
        {
            Value = new StartupDataDto(
                new UserContextDto(_financeUserContext.RangeStart, _financeUserContext.RangeEnd),
                accountTypes,
                currencies,
                transactionTypes)
        };

        return response;
    }
}
