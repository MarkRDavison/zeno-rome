namespace mark.davison.rome.api.queries.tests.Scenarios.TransactionByAccount;

public sealed class TransactionByAccountQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly TransactionByAccountQueryProcessor _transactionByAccountQueryProcessor;

    public TransactionByAccountQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);

        _transactionByAccountQueryProcessor = new(_dbContext);
    }

    [Test]
    public async Task ProcessAsync_ReturnsTransactionsForAccount()
    {
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        var transactionGroup = new TransactionGroup
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };
        var transactionJournal = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = transactionGroup.Id,
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionJournalId = transactionJournal.Id,
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = Guid.Empty,
                IsSource = true,
                AccountId = sourceAccountId
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionJournalId = transactionJournal.Id,
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = Guid.Empty,
                IsSource = false,
                AccountId = destinationAccountId
            }
        };

        {
            await _dbContext.AddAsync(transactionGroup, CancellationToken.None);
            await _dbContext.AddAsync(transactionJournal, CancellationToken.None);

            foreach (var t in transactions)
            {
                await _dbContext.AddAsync(t, CancellationToken.None);
            }

            await _dbContext.SaveChangesAsync(true, CancellationToken.None);
        }

        var request = new TransactionByAccountQueryRequest { AccountId = sourceAccountId };

        var response = await _transactionByAccountQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);
    }
}
