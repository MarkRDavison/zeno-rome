using mark.davison.rome.api.models.Entities;

namespace mark.davison.rome.api.queries.tests.Scenarios.FetchTransaction;

public class FetchTransactionQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly FetchTransactionQueryProcessor _fetchTransactionQueryProcessor;

    public FetchTransactionQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);

        _fetchTransactionQueryProcessor = new(_dbContext);
    }

    [Test]
    public async Task ProcessAsync_WhereTransactionExists_ReturnsTransaction()
    {
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
                IsSource = true
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionJournalId = transactionJournal.Id,
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = Guid.Empty,
                IsSource = false
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

        var request = new FetchTransactionQueryRequest { TransactionGroupId = transactionGroup.Id };

        var response = await _fetchTransactionQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);
    }

    [Test]
    public async Task ProcessAsync_WhereTransactionDoesNotExist_ReturnsEmpty()
    {
        var request = new FetchTransactionQueryRequest { TransactionGroupId = Guid.NewGuid() };

        var response = await _fetchTransactionQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(0);
    }
}
