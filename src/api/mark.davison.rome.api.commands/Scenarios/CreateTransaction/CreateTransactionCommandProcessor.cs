namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction;

public sealed class CreateTransactionCommandProcessor : ICommandProcessor<CreateTransactionCommandRequest, CreateTransactionCommandResponse>
{
    private readonly IDateService _dateService;
    private readonly IDbContext<RomeDbContext> _dbContext;

    public CreateTransactionCommandProcessor(
        IDateService dateService,
        IDbContext<RomeDbContext> dbContext
    )
    {
        _dateService = dateService;
        _dbContext = dbContext;
    }

    public async Task<CreateTransactionCommandResponse> ProcessAsync(CreateTransactionCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var response = new CreateTransactionCommandResponse();
        var transactionGroup = new TransactionGroup
        {
            Id = Guid.NewGuid(),
            Title = request.Transactions.Count > 1
                ? request.Description
                : string.Empty,
            UserId = currentUserContext.UserId,
            Created = _dateService.Now,
            LastModified = _dateService.Now
        };

        int order = 0;
        var journals = new List<TransactionJournal>();
        var transactions = new List<Transaction>();

        foreach (var transaction in request.Transactions)
        {
            var transactionJournal = new TransactionJournal
            {
                Id = Guid.NewGuid(),
                UserId = currentUserContext.UserId,
                TransactionGroupId = transactionGroup.Id,
                Description = transaction.Description,
                TransactionTypeId = request.TransactionTypeId,
                CurrencyId = transaction.CurrencyId,
                ForeignCurrencyId = transaction.ForeignCurrencyId,
                Order = order++,
                Date = transaction.Date,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            };

            var sourceTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = currentUserContext.UserId,
                TransactionJournalId = transactionJournal.Id,
                AccountId = transaction.SourceAccountId,
                CurrencyId = transaction.CurrencyId,
                ForeignCurrencyId = transaction.ForeignCurrencyId,
                Description = transaction.Description,
                Amount = -transaction.Amount,
                ForeignAmount = -transaction.ForeignAmount,
                IsSource = true,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            };
            var destinationTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = currentUserContext.UserId,
                TransactionJournalId = transactionJournal.Id,
                AccountId = transaction.DestinationAccountId,
                CurrencyId = transaction.CurrencyId,
                ForeignCurrencyId = transaction.ForeignCurrencyId,
                Description = transaction.Description,
                Amount = transaction.Amount,
                ForeignAmount = transaction.ForeignAmount,
                IsSource = false,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            };

            transactions.Add(sourceTransaction);
            transactions.Add(destinationTransaction);
            journals.Add(transactionJournal);
        }

        await _dbContext.UpsertEntityAsync(transactionGroup, cancellationToken);
        await _dbContext.UpsertEntitiesAsync(journals, cancellationToken);
        await _dbContext.UpsertEntitiesAsync(transactions, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        response.Group = new TransactionGroupDto(transactionGroup.Id, transactionGroup.Title);
        response.Journals.AddRange(journals.Select(_ => new TransactionJournalDto(_.Id)));
        response.Transactions.AddRange(transactions.Select(_ => CreateTransactionDto(_, journals.First(__ => __.Id == _.TransactionJournalId), transactionGroup)));

        return response;
    }

    private static TransactionDto CreateTransactionDto(Transaction transaction, TransactionJournal journal, TransactionGroup group)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Date = journal.Date,
            CategoryId = null,//journal.CategoryId,
            CurrencyId = transaction.CurrencyId,
            Description = transaction.Description,
            ForeignAmount = transaction.ForeignAmount,
            ForeignCurrencyId = transaction.ForeignCurrencyId,
            Reconciled = transaction.Reconciled,
            Source = transaction.IsSource,
            UserId = transaction.UserId,
            SplitTransactionDescription = group.Title,
            TransactionGroupId = group.Id,
            TransactionJournalId = transaction.TransactionJournalId,
            TransactionTypeId = journal.TransactionTypeId
        };
    }
}