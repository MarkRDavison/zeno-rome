namespace mark.davison.rome.api.queries.Scenarios.TransactionByAccount;

public sealed class TransactionByAccountQueryProcessor : IQueryProcessor<TransactionByAccountQueryRequest, TransactionByAccountQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IFinanceUserContext _financeUserContext;

    public TransactionByAccountQueryProcessor(
        IDbContext<RomeDbContext> dbContext,
        IFinanceUserContext financeUserContext)
    {
        _dbContext = dbContext;
        _financeUserContext = financeUserContext;
    }

    public async Task<TransactionByAccountQueryResponse> ProcessAsync(TransactionByAccountQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var rangeStart = _financeUserContext.RangeStart;
        var rangeEnd = _financeUserContext.RangeEnd;

        var transactionJournals = await _dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .Include(_ => _.TransactionGroup)
            .Include(_ => _.JournalTags)
                .ThenInclude(jt => jt.Tag)
            .Where(_ =>
                _.Date >= rangeStart &&
                _.Date <= rangeEnd &&
                _.Transactions.Any(t => t.AccountId == request.AccountId))
            .OrderByDescending(_ => _.Date)
            .ThenBy(_ => _.Order)
            .ToListAsync(cancellationToken);

        var dtos = new List<TransactionDto>();

        foreach (var journal in transactionJournals)
        {
            var group = journal.TransactionGroup!;
            dtos.AddRange(journal.Transactions.Select(t => t.ToDto(journal, group)));
        }

        return new TransactionByAccountQueryResponse
        {
            Value = dtos
        };
    }
}
