namespace mark.davison.rome.api.tests.Scenarios.TransactionByAccount;

public sealed class TransactionByAccountIntegrationTests : ApiIntegrationTestBase
{
    [Test]
    public async Task MultiSplitWithdrawal_QueryByAccount_MatchesBalanceAndMetadata()
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var dbContext = GetRequiredService<IDbContext<RomeDbContext>>();

        var currencyId = await dbContext
            .Set<Currency>()
            .AsNoTracking()
            .Select(_ => _.Id)
            .FirstAsync(CancellationToken.None);

        var assetAccountId = Guid.NewGuid();
        var expenseAccount1Id = Guid.NewGuid();
        var expenseAccount2Id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var userId = currentUser.UserId;
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await dbContext.AddAsync(new Account
        {
            Id = assetAccountId,
            Name = "Integration Asset",
            AccountTypeId = AccountTypeConstants.Asset,
            CurrencyId = currencyId,
            UserId = userId,
            Created = now,
            LastModified = now
        }, CancellationToken.None);

        await dbContext.AddAsync(new Account
        {
            Id = expenseAccount1Id,
            Name = "Integration Expense 1",
            AccountTypeId = AccountTypeConstants.Expense,
            CurrencyId = currencyId,
            UserId = userId,
            Created = now,
            LastModified = now
        }, CancellationToken.None);

        await dbContext.AddAsync(new Account
        {
            Id = expenseAccount2Id,
            Name = "Integration Expense 2",
            AccountTypeId = AccountTypeConstants.Expense,
            CurrencyId = currencyId,
            UserId = userId,
            Created = now,
            LastModified = now
        }, CancellationToken.None);

        await dbContext.AddAsync(new Category
        {
            Id = categoryId,
            Name = "Integration Food",
            UserId = userId,
            Created = now,
            LastModified = now
        }, CancellationToken.None);

        var group = new TransactionGroup
        {
            Id = Guid.NewGuid(),
            Title = "Integration split",
            UserId = userId,
            Created = now,
            LastModified = now
        };

        var journal0 = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = group.Id,
            TransactionTypeId = TransactionTypeConstants.Withdrawal,
            CurrencyId = currencyId,
            CategoryId = categoryId,
            Order = 0,
            Date = today,
            UserId = userId,
            Created = now,
            LastModified = now
        };

        var journal1 = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = group.Id,
            TransactionTypeId = TransactionTypeConstants.Withdrawal,
            CurrencyId = currencyId,
            Order = 1,
            Date = today,
            UserId = userId,
            Created = now,
            LastModified = now
        };

        await dbContext.AddAsync(group, CancellationToken.None);
        await dbContext.AddAsync(journal0, CancellationToken.None);
        await dbContext.AddAsync(journal1, CancellationToken.None);

        await dbContext.AddAsync(CreateLeg(journal0.Id, assetAccountId, -1_000_000, true, userId, currencyId, now), CancellationToken.None);
        await dbContext.AddAsync(CreateLeg(journal0.Id, expenseAccount1Id, 1_000_000, false, userId, currencyId, now), CancellationToken.None);
        await dbContext.AddAsync(CreateLeg(journal1.Id, assetAccountId, -500_000, true, userId, currencyId, now), CancellationToken.None);
        await dbContext.AddAsync(CreateLeg(journal1.Id, expenseAccount2Id, 500_000, false, userId, currencyId, now), CancellationToken.None);

        await dbContext.SaveChangesAsync(true, CancellationToken.None);

        var financeUserContext = GetRequiredService<IFinanceUserContext>();
        await financeUserContext.LoadAsync(CancellationToken.None);
        await financeUserContext.SetAsync(today.AddYears(-1), today.AddYears(1), CancellationToken.None);

        var queryHandler = GetRequiredService<IQueryHandler<TransactionByAccountQueryRequest, TransactionByAccountQueryResponse>>();
        var queryResponse = await queryHandler.Handle(
            new TransactionByAccountQueryRequest { AccountId = assetAccountId },
            currentUser,
            CancellationToken.None);

        await Assert.That(queryResponse.SuccessWithValue).IsTrue();
        var assetLegs = queryResponse.Value.Where(_ => _.AccountId == assetAccountId).ToList();
        await Assert.That(assetLegs).Count().IsEqualTo(2);
        await Assert.That(assetLegs[0].JournalOrder).IsEqualTo(0);
        await Assert.That(assetLegs[1].JournalOrder).IsEqualTo(1);
        await Assert.That(assetLegs[0].CategoryId).IsEqualTo(categoryId);
        await Assert.That(assetLegs[0].SplitTransactionDescription).IsEqualTo("Integration split");

        var accountListHandler = GetRequiredService<IQueryHandler<AccountListQueryRequest, AccountListQueryResponse>>();
        await financeUserContext.LoadAsync(CancellationToken.None);

        var accountList = await accountListHandler.Handle(
            new AccountListQueryRequest { AccountType = null },
            currentUser,
            CancellationToken.None);

        var asset = accountList.Value.Single(_ => _.Id == assetAccountId);
        await Assert.That(asset.CurrentBalance).IsEqualTo(-1_500_000);
    }

    private static Transaction CreateLeg(
        Guid journalId,
        Guid accountId,
        long amount,
        bool isSource,
        Guid userId,
        Guid currencyId,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            TransactionJournalId = journalId,
            AccountId = accountId,
            Amount = amount,
            IsSource = isSource,
            CurrencyId = currencyId,
            UserId = userId,
            Created = now,
            LastModified = now
        };
}
