using Microsoft.EntityFrameworkCore;

namespace mark.davison.rome.api.queries.tests.Scenarios.AccountById;

public sealed class AccountByIdQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IFinanceUserContext> _financeUserContextMock;
    private readonly AccountByIdQueryProcessor _processor;
    public AccountByIdQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _currentUserContextMock.Setup(_ => _.UserId).Returns(Guid.Empty);

        _financeUserContextMock = new(MockBehavior.Strict);
        var today = DateOnly.FromDateTime(DateTime.Today);
        _financeUserContextMock.Setup(_ => _.RangeStart).Returns(today.AddYears(-1));
        _financeUserContextMock.Setup(_ => _.RangeEnd).Returns(today.AddYears(1));

        _processor = new AccountByIdQueryProcessor(_dbContext, _financeUserContextMock.Object);
    }

    [Test]
    public async Task ProcessAsync_WhenAccountExists_ReturnsAccountDto()
    {
        var accountId = Guid.NewGuid();
        await _dbContext.AddAsync(new Account
        {
            Id = accountId,
            Name = "Checking",
            AccountTypeId = AccountTypeConstants.Asset,
            AccountNumber = "123",
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            IsActive = true,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var persisted = await _dbContext
            .Set<Account>()
            .FirstOrDefaultAsync(_ => _.Id == accountId && _.UserId == Guid.Empty);
        await Assert.That(persisted).IsNotNull();

        var response = await _processor.ProcessAsync(
            new AccountByIdQueryRequest { AccountId = accountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.Errors).IsEmpty();
        await Assert.That(response.Value).IsNotNull();
        await Assert.That(response.Value!.Id).IsEqualTo(accountId);
        await Assert.That(response.Value.Name).IsEqualTo("Checking");
    }

    [Test]
    public async Task ProcessAsync_WhenAccountMissing_ReturnsError()
    {
        var response = await _processor.ProcessAsync(
            new AccountByIdQueryRequest { AccountId = Guid.NewGuid() },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsFalse();
        await Assert.That(response.Errors).Contains("ACCOUNT_NOT_FOUND");
    }

    [Test]
    public async Task ProcessAsync_WhenAccountBelongsToOtherUser_ReturnsError()
    {
        var accountId = Guid.NewGuid();
        await _dbContext.AddAsync(new Account
        {
            Id = accountId,
            Name = "Other user",
            AccountTypeId = AccountTypeConstants.Asset,
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var response = await _processor.ProcessAsync(
            new AccountByIdQueryRequest { AccountId = accountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsFalse();
        await Assert.That(response.Errors).Contains("ACCOUNT_NOT_FOUND");
    }

    [Test]
    public async Task ProcessAsync_BalanceDifference_IsEndBalanceMinusStartBalance()
    {
        var accountId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var rangeStart = new DateOnly(2026, 3, 1);
        var rangeEnd = new DateOnly(2026, 3, 31);
        var beforeRange = new DateOnly(2026, 2, 15);
        var inRange = new DateOnly(2026, 3, 10);
        var afterRange = new DateOnly(2026, 4, 5);

        await _dbContext.AddAsync(new Account
        {
            Id = accountId,
            Name = "Checking",
            AccountTypeId = AccountTypeConstants.Asset,
            AccountNumber = "123",
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            IsActive = true,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);

        await SeedExpenseJournal(accountId, expenseId, beforeRange, -1_000_000);
        await SeedExpenseJournal(accountId, expenseId, inRange, -500_000);
        await SeedExpenseJournal(accountId, expenseId, afterRange, -200_000);

        var financeUserContextMock = new Mock<IFinanceUserContext>(MockBehavior.Strict);
        financeUserContextMock.Setup(_ => _.RangeStart).Returns(rangeStart);
        financeUserContextMock.Setup(_ => _.RangeEnd).Returns(rangeEnd);

        var processor = new AccountByIdQueryProcessor(_dbContext, financeUserContextMock.Object);

        var response = await processor.ProcessAsync(
            new AccountByIdQueryRequest { AccountId = accountId },
            _currentUserContextMock.Object,
            CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        // End balance: all txns through 31 Mar = -1.5M
        await Assert.That(response.Value!.CurrentBalance).IsEqualTo(-1_500_000);
        // Start balance (before 1 Mar): -1M; change in March = -500k
        await Assert.That(response.Value.BalanceDifference).IsEqualTo(-500_000);
    }

    private async Task SeedExpenseJournal(Guid assetId, Guid expenseId, DateOnly date, long assetAmount)
    {
        var groupId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        await _dbContext.AddAsync(new TransactionGroup
        {
            Id = groupId,
            Title = string.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            UserId = Guid.Empty
        }, CancellationToken.None);
        await _dbContext.AddAsync(new TransactionJournal
        {
            Id = journalId,
            TransactionGroupId = groupId,
            Order = 0,
            Date = date,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            UserId = Guid.Empty
        }, CancellationToken.None);
        await _dbContext.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionJournalId = journalId,
            AccountId = assetId,
            Amount = assetAmount,
            IsSource = true,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.AddAsync(new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionJournalId = journalId,
            AccountId = expenseId,
            Amount = -assetAmount,
            IsSource = false,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);
    }
}
