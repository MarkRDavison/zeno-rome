namespace mark.davison.rome.api.queries.Scenarios.FetchTransaction;

public sealed class FetchTransactionQueryProcessor : IQueryProcessor<FetchTransactionQueryRequest, FetchTransactionQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    public FetchTransactionQueryProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FetchTransactionQueryResponse> ProcessAsync(FetchTransactionQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var response = new FetchTransactionQueryResponse();

        var transactionJournals = await _dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .Include(_ => _.TransactionGroup)
            .Where(_ => _.TransactionGroupId == request.TransactionGroupId)
            .ToListAsync(cancellationToken);

        response.Value = [];

        foreach (var tj in transactionJournals)
        {
            response.Value
                .AddRange(tj.Transactions
                    .Select(
                        _ => _.ToDto(tj, tj.TransactionGroup!)));
        }

        return response;
    }
}
