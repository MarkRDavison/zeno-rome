namespace mark.davison.rome.api.queries.tests.Scenarios.TransactionByAccount;

public sealed class TransactionByAccountQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IFinanceUserContext> _financeUserContextMock;
    private readonly TransactionByAccountQueryProcessor _transactionByAccountQueryProcessor;

    public TransactionByAccountQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _financeUserContextMock = new(MockBehavior.Strict);
        var today = DateOnly.FromDateTime(DateTime.Today);
        _financeUserContextMock.Setup(_ => _.RangeStart).Returns(today.AddYears(-10));
        _financeUserContextMock.Setup(_ => _.RangeEnd).Returns(today.AddYears(10));

        _transactionByAccountQueryProcessor = new(_dbContext, _financeUserContextMock.Object);
    }

    [Test]
    public async Task ProcessAsync_ReturnsTransactionsForAccount()
    {
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        await SeedJournalPair(sourceAccountId, destinationAccountId, order: 0, categoryId: null, groupTitle: string.Empty);

        var request = new TransactionByAccountQueryRequest { AccountId = sourceAccountId };

        var response = await _transactionByAccountQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);
    }

    [Test]
    public async Task ProcessAsync_MultiJournalSplit_OrdersByJournalOrderAndMapsCategory()
    {
        var assetAccountId = Guid.NewGuid();
        var expense1Id = Guid.NewGuid();
        var expense2Id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var transactionGroup = new TransactionGroup
        {
            Id = groupId,
            Title = "Split shop",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };

        var journal0 = CreateJournal(groupId, order: 0, date: today, categoryId: categoryId);
        var journal1 = CreateJournal(groupId, order: 1, date: today, categoryId: null);

        await _dbContext.AddAsync(transactionGroup, CancellationToken.None);
        await _dbContext.AddAsync(journal0, CancellationToken.None);
        await _dbContext.AddAsync(journal1, CancellationToken.None);

        await _dbContext.AddAsync(CreateLeg(journal0.Id, assetAccountId, -1000, true), CancellationToken.None);
        await _dbContext.AddAsync(CreateLeg(journal0.Id, expense1Id, 1000, false), CancellationToken.None);
        await _dbContext.AddAsync(CreateLeg(journal1.Id, assetAccountId, -2000, true), CancellationToken.None);
        await _dbContext.AddAsync(CreateLeg(journal1.Id, expense2Id, 2000, false), CancellationToken.None);

        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await _transactionByAccountQueryProcessor.ProcessAsync(
            new TransactionByAccountQueryRequest { AccountId = assetAccountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.Value).Count().IsEqualTo(4);

        var assetRows = response.Value.Where(_ => _.AccountId == assetAccountId).ToList();
        await Assert.That(assetRows).Count().IsEqualTo(2);
        await Assert.That(assetRows[0].JournalOrder).IsEqualTo(0);
        await Assert.That(assetRows[1].JournalOrder).IsEqualTo(1);
        await Assert.That(assetRows[0].CategoryId).IsEqualTo(categoryId);
        await Assert.That(assetRows[0].SplitTransactionDescription).IsEqualTo("Split shop");

        await Assert.That(response.Value.Any(_ => _.AccountId == expense1Id)).IsTrue();
        await Assert.That(response.Value.Any(_ => _.AccountId == expense2Id)).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_ExcludesJournalsOutsideDateRange()
    {
        var assetAccountId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var inRangeDate = new DateOnly(2026, 3, 15);
        var outOfRangeDate = new DateOnly(2026, 1, 10);

        await SeedJournalPair(assetAccountId, expenseId, order: 0, categoryId: null, groupTitle: string.Empty, inRangeDate);
        await SeedJournalPair(assetAccountId, expenseId, order: 0, categoryId: null, groupTitle: string.Empty, outOfRangeDate);

        _financeUserContextMock.Setup(_ => _.RangeStart).Returns(new DateOnly(2026, 3, 1));
        _financeUserContextMock.Setup(_ => _.RangeEnd).Returns(new DateOnly(2026, 3, 31));

        var response = await _transactionByAccountQueryProcessor.ProcessAsync(
            new TransactionByAccountQueryRequest { AccountId = assetAccountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.Value).Count().IsEqualTo(2);
        await Assert.That(response.Value.All(_ => _.Date == inRangeDate)).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_DoesNotReturnJournalsForOtherAccounts()
    {
        var assetAccountId = Guid.NewGuid();
        var otherAssetId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();

        await SeedJournalPair(assetAccountId, expenseId, order: 0, categoryId: null, groupTitle: string.Empty);
        await SeedJournalPair(otherAssetId, expenseId, order: 0, categoryId: null, groupTitle: string.Empty);

        var response = await _transactionByAccountQueryProcessor.ProcessAsync(
            new TransactionByAccountQueryRequest { AccountId = assetAccountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.Value).Count().IsEqualTo(2);
        await Assert.That(response.Value.All(_ => _.AccountId == assetAccountId || _.AccountId == expenseId)).IsTrue();
        await Assert.That(response.Value.Any(_ => _.AccountId == otherAssetId)).IsFalse();
    }

    private async Task SeedJournalPair(
        Guid sourceAccountId,
        Guid destinationAccountId,
        int order,
        Guid? categoryId,
        string groupTitle,
        DateOnly? date = null)
    {
        var journalDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var transactionGroup = new TransactionGroup
        {
            Id = Guid.NewGuid(),
            Title = groupTitle,
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };
        var transactionJournal = CreateJournal(transactionGroup.Id, order, journalDate, categoryId);

        await _dbContext.AddAsync(transactionGroup, CancellationToken.None);
        await _dbContext.AddAsync(transactionJournal, CancellationToken.None);
        await _dbContext.AddAsync(CreateLeg(transactionJournal.Id, sourceAccountId, -100, true), CancellationToken.None);
        await _dbContext.AddAsync(CreateLeg(transactionJournal.Id, destinationAccountId, 100, false), CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);
    }

    private static TransactionJournal CreateJournal(Guid groupId, int order, DateOnly date, Guid? categoryId) =>
        new()
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = groupId,
            Order = order,
            Date = date,
            CategoryId = categoryId,
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty
        };

    private static Transaction CreateLeg(Guid journalId, Guid accountId, long amount, bool isSource) =>
        new()
        {
            Id = Guid.NewGuid(),
            TransactionJournalId = journalId,
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.Empty,
            IsSource = isSource,
            AccountId = accountId,
            Amount = amount
        };
}
