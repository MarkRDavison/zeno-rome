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
    public async Task ProcessAsync_WhereJournalHasTags_ReturnsTagsOnEachTransactionLine()
    {
        var tagAId = Guid.NewGuid();
        var tagBId = Guid.NewGuid();
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
        var tagA = new Tag
        {
            Id = tagAId,
            Name = "Alpha",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };
        var tagB = new Tag
        {
            Id = tagBId,
            Name = "Beta",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TransactionJournalId = transactionJournal.Id,
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = Guid.Empty,
                IsSource = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TransactionJournalId = transactionJournal.Id,
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = Guid.Empty,
                IsSource = false
            }
        };

        await _dbContext.AddAsync(transactionGroup, CancellationToken.None);
        await _dbContext.AddAsync(transactionJournal, CancellationToken.None);
        await _dbContext.AddAsync(tagA, CancellationToken.None);
        await _dbContext.AddAsync(tagB, CancellationToken.None);

        foreach (var t in transactions)
        {
            await _dbContext.AddAsync(t, CancellationToken.None);
        }

        await _dbContext.AddAsync(new TransactionJournalTag
        {
            TransactionJournalId = transactionJournal.Id,
            TagId = tagBId
        }, CancellationToken.None);
        await _dbContext.AddAsync(new TransactionJournalTag
        {
            TransactionJournalId = transactionJournal.Id,
            TagId = tagAId
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var request = new FetchTransactionQueryRequest { TransactionGroupId = transactionGroup.Id };
        var response = await _fetchTransactionQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);

        foreach (var dto in response.Value!)
        {
            await Assert.That(dto.Tags).Count().IsEqualTo(2);
            await Assert.That(dto.Tags[0].Name).IsEqualTo("Alpha");
            await Assert.That(dto.Tags[1].Name).IsEqualTo("Beta");
            await Assert.That(dto.Tags[0].Id).IsEqualTo(tagAId);
            await Assert.That(dto.Tags[1].Id).IsEqualTo(tagBId);
        }
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
