namespace mark.davison.rome.api.tests.Scenarios.UpsertAccount;

public sealed class UpsertAccountOpeningBalanceIntegrationTests : ApiIntegrationTestBase
{
    [Test]
    public async Task CreateAssetAccount_WithOpeningBalance_CreatesJournalAndBalance()
    {
        await AssertCreateWithOpeningBalance(
            AccountTypeConstants.Asset,
            2_500_000,
            expectedBalance: 2_500_000);
    }

    [Test]
    public async Task CreateExpenseAccount_WithOpeningBalance_CreatesJournalAndBalance()
    {
        await AssertCreateWithOpeningBalance(
            AccountTypeConstants.Expense,
            500_000,
            expectedBalance: 500_000);
    }

    private async Task AssertCreateWithOpeningBalance(Guid accountTypeId, long openingBalance, long expectedBalance)
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var dbContext = GetRequiredService<IDbContext<RomeDbContext>>();

        var currencyId = CurrencyConstants.NZD;
        var accountId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var financeUserContext = GetRequiredService<IFinanceUserContext>();
        await financeUserContext.LoadAsync(CancellationToken.None);
        await financeUserContext.SetAsync(today.AddYears(-1), today.AddYears(1), CancellationToken.None);

        var handler = GetRequiredService<ICommandHandler<UpsertAccountCommandRequest, UpsertAccountCommandResponse>>();

        var response = await handler.Handle(
            new UpsertAccountCommandRequest
            {
                UpsertAccountDto = new UpsertAccountDto(
                    accountId,
                    $"Integration {accountTypeId:N}",
                    null,
                    string.Empty,
                    accountTypeId,
                    currencyId,
                    openingBalance,
                    today)
            },
            currentUser,
            CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
        await Assert.That(response.SuccessWithValue).IsTrue();

        var openingJournal = await dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .FirstOrDefaultAsync(_ =>
                _.TransactionTypeId == TransactionTypeConstants.OpeningBalance &&
                _.Transactions.Any(t => t.AccountId == accountId),
                CancellationToken.None);

        await Assert.That(openingJournal).IsNotNull();
        await Assert.That(openingJournal!.Date).IsEqualTo(today);

        var accountLeg = openingJournal.Transactions.First(t => t.AccountId == accountId);
        await Assert.That(accountLeg.Amount).IsEqualTo(expectedBalance);

        var accountListHandler = GetRequiredService<IQueryHandler<AccountListQueryRequest, AccountListQueryResponse>>();
        var accountList = await accountListHandler.Handle(
            new AccountListQueryRequest { AccountType = null },
            currentUser,
            CancellationToken.None);

        var account = accountList.Value.Single(_ => _.Id == accountId);
        await Assert.That(account.CurrentBalance).IsEqualTo(expectedBalance);
    }
}
