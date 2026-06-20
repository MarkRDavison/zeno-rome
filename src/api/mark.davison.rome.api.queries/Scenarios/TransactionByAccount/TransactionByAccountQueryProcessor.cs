namespace mark.davison.rome.api.queries.Scenarios.TransactionByAccount;

public sealed class TransactionByAccountQueryProcessor : IQueryProcessor<TransactionByAccountQueryRequest, TransactionByAccountQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    public TransactionByAccountQueryProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionByAccountQueryResponse> ProcessAsync(TransactionByAccountQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var response = new TransactionByAccountQueryResponse
        {
            Value = []
        };

        var transactionJournals = await _dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .Include(_ => _.TransactionGroup)
            .Where(_ => _.Transactions.Any(t => t.AccountId == request.AccountId))
            .ToListAsync(cancellationToken);

        // TODO: Return transaction journal??? Need source/destination account etc

        foreach (var tj in transactionJournals)
        {
            var tg = tj.TransactionGroup;

            response.Value.AddRange(tj.Transactions.Select(_ => new TransactionDto
            {
                Id = _.Id,
                UserId = _.UserId,
                AccountId = _.AccountId,
                TransactionJournalId = _.TransactionJournalId,
                TransactionGroupId = tj.TransactionGroupId,
                CurrencyId = _.CurrencyId,
                ForeignCurrencyId = _.ForeignCurrencyId,
                CategoryId = null, //_.TransactionJournal?.CategoryId,
                SplitTransactionDescription = tg?.Title ?? string.Empty,
                Description = _.Description,
                Date = _.TransactionJournal!.Date,
                Amount = _.Amount,
                ForeignAmount = _.ForeignAmount,
                Reconciled = _.Reconciled,
                Source = _.IsSource,
                TransactionTypeId = tj.TransactionTypeId
            }));
        }

        return response;
    }
}
