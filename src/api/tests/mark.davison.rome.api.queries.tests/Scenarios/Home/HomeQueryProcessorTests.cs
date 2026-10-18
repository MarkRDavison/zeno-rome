namespace mark.davison.rome.api.queries.tests.Scenarios.Home;

public sealed class HomeQueryProcessorTests
{
    private static (HomeQueryProcessor Processor, IDbContext<RomeDbContext> Db, Mock<ICurrentUserContext> User) CreateSut()
    {
        var dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        var userContextMock = new Mock<ICurrentUserContext>(MockBehavior.Strict);
        userContextMock.Setup(_ => _.UserId).Returns(Guid.Empty);

        var financeUserContextMock = new Mock<IFinanceUserContext>(MockBehavior.Strict);
        financeUserContextMock.Setup(_ => _.RangeStart).Returns(new DateOnly(2020, 1, 1));
        financeUserContextMock.Setup(_ => _.RangeEnd).Returns(new DateOnly(2030, 12, 31));

        var processor = new HomeQueryProcessor(dbContext, financeUserContextMock.Object);
        return (processor, dbContext, userContextMock);
    }

    [Test]
    public async Task ProcessAsync_WhenNoFavourites_ReturnsAssetAccountsOnly()
    {
        var (processor, dbContext, userContext) = CreateSut();

        var asset = CreateAccount("Asset", AccountTypeConstants.Asset, isFavourite: false);
        var expense = CreateAccount("Expense", AccountTypeConstants.Expense, isFavourite: false);
        await dbContext.AddAsync(asset, CancellationToken.None);
        await dbContext.AddAsync(expense, CancellationToken.None);
        await dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await processor.ProcessAsync(new HomeQueryRequest(), userContext.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value.Accounts.Select(_ => _.Id)).IsEquivalentTo([asset.Id]);
    }

    [Test]
    public async Task ProcessAsync_WhenAnyFavourite_ReturnsFavouritesOnly_IncludingInactive()
    {
        var (processor, dbContext, userContext) = CreateSut();

        var favouriteExpense = CreateAccount("Fav expense", AccountTypeConstants.Expense, isFavourite: true, isActive: false);
        var asset = CreateAccount("Asset", AccountTypeConstants.Asset, isFavourite: false);
        await dbContext.Set<Account>().AddRangeAsync([favouriteExpense, asset], CancellationToken.None);
        await dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await processor.ProcessAsync(new HomeQueryRequest(), userContext.Object, CancellationToken.None);

        await Assert.That(response.Value.Accounts.Select(_ => _.Id)).IsEquivalentTo([favouriteExpense.Id]);
    }

    [Test]
    public async Task ProcessAsync_RecentTransactions_UsesJournalDescriptionWhenGroupTitleEmpty()
    {
        var (processor, dbContext, userContext) = CreateSut();

        var group = new TransactionGroup { Id = Guid.NewGuid(), UserId = Guid.Empty, Title = string.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow };
        var journal = CreateJournal(group.Id, new DateOnly(2025, 6, 1));
        journal.Description = "Coffee shop";

        await dbContext.AddAsync(group, CancellationToken.None);
        await dbContext.AddAsync(journal, CancellationToken.None);
        await dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await processor.ProcessAsync(new HomeQueryRequest(), userContext.Object, CancellationToken.None);

        await Assert.That(response.Value!.RecentTransactions.Single().Title).IsEqualTo("Coffee shop");
    }

    [Test]
    public async Task ProcessAsync_RecentTransactions_RespectsRangeAndOrdersByDate()
    {
        var (processor, dbContext, userContext) = CreateSut();

        var group1 = new TransactionGroup { Id = Guid.NewGuid(), UserId = Guid.Empty, Title = "G1", Created = DateTime.UtcNow, LastModified = DateTime.UtcNow };
        var group2 = new TransactionGroup { Id = Guid.NewGuid(), UserId = Guid.Empty, Title = "G2", Created = DateTime.UtcNow, LastModified = DateTime.UtcNow };
        var group3 = new TransactionGroup { Id = Guid.NewGuid(), UserId = Guid.Empty, Title = "G3", Created = DateTime.UtcNow, LastModified = DateTime.UtcNow };

        var journals = new[]
        {
            CreateJournal(group1.Id, new DateOnly(2025, 6, 1)),
            CreateJournal(group2.Id, new DateOnly(2025, 7, 1)),
            CreateJournal(group3.Id, new DateOnly(2025, 8, 1)),
            CreateJournal(group3.Id, new DateOnly(2019, 1, 1)),
        };

        await dbContext.Set<TransactionGroup>().AddRangeAsync([group1, group2, group3], CancellationToken.None);
        await dbContext.Set<TransactionJournal>().AddRangeAsync(journals, CancellationToken.None);
        await dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await processor.ProcessAsync(new HomeQueryRequest(), userContext.Object, CancellationToken.None);

        await Assert.That(response.Value.RecentTransactions).Count().IsEqualTo(3);
        await Assert.That(response.Value.RecentTransactions[0].TransactionGroupId).IsEqualTo(group3.Id);
        await Assert.That(response.Value.RecentTransactions[1].TransactionGroupId).IsEqualTo(group2.Id);
        await Assert.That(response.Value.RecentTransactions[2].TransactionGroupId).IsEqualTo(group1.Id);
    }

    private static Account CreateAccount(string name, Guid accountTypeId, bool isFavourite, bool isActive = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = Guid.Empty,
            AccountTypeId = accountTypeId,
            CurrencyId = CurrencyConstants.NZD,
            AccountNumber = string.Empty,
            IsFavourite = isFavourite,
            IsActive = isActive,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            Order = 0
        };

    private static TransactionJournal CreateJournal(Guid groupId, DateOnly date) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty,
            TransactionGroupId = groupId,
            TransactionTypeId = TransactionTypeConstants.Deposit,
            CurrencyId = CurrencyConstants.NZD,
            Date = date,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
}
