namespace mark.davison.rome.api.tests.Scenarios.UpsertAccount;

public sealed class UpsertAccountEditOpeningBalanceIntegrationTests : ApiIntegrationTestBase
{
    [Test]
    public async Task EditOpeningBalance_UpdatesJournalAndAccountList()
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var dbContext = GetRequiredService<IDbContext<RomeDbContext>>();
        var handler = GetRequiredService<ICommandHandler<UpsertAccountCommandRequest, UpsertAccountCommandResponse>>();

        var accountId = Guid.NewGuid();
        var initialDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var updatedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

        await SetFinanceRangeIncluding(initialDate, updatedDate);

        await Upsert(handler, currentUser, accountId, 1_000_000, initialDate, "Before edit");

        var editResponse = await handler.Handle(
            new UpsertAccountCommandRequest
            {
                UpsertAccountDto = new UpsertAccountDto(
                    accountId,
                    "After edit",
                    null,
                    string.Empty,
                    AccountTypeConstants.Asset,
                    CurrencyConstants.NZD,
                    3_000_000,
                    updatedDate)
            },
            currentUser,
            CancellationToken.None);

        await Assert.That(editResponse.Success).IsTrue();

        var openingJournal = await GetOpeningJournal(dbContext, accountId);
        await Assert.That(openingJournal!.Date).IsEqualTo(updatedDate);

        var accountLeg = openingJournal.Transactions.First(t => t.AccountId == accountId);
        await Assert.That(accountLeg.Amount).IsEqualTo(3_000_000);

        var account = await GetAccountFromList(currentUser, accountId);
        await Assert.That(account.CurrentBalance).IsEqualTo(3_000_000);
        await Assert.That(account.OpeningBalance).IsEqualTo(3_000_000);
        await Assert.That(account.OpeningBalanceDate).IsEqualTo(updatedDate);
    }

    [Test]
    public async Task RemoveOpeningBalance_DeletesJournal()
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var dbContext = GetRequiredService<IDbContext<RomeDbContext>>();
        var handler = GetRequiredService<ICommandHandler<UpsertAccountCommandRequest, UpsertAccountCommandResponse>>();

        var accountId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SetFinanceRangeIncluding(today);

        await Upsert(handler, currentUser, accountId, 500_000, today, "With opening");

        var removeResponse = await handler.Handle(
            new UpsertAccountCommandRequest
            {
                UpsertAccountDto = new UpsertAccountDto(
                    accountId,
                    "With opening",
                    null,
                    string.Empty,
                    AccountTypeConstants.Asset,
                    CurrencyConstants.NZD,
                    null,
                    null)
            },
            currentUser,
            CancellationToken.None);

        await Assert.That(removeResponse.Success).IsTrue();

        var openingJournal = await GetOpeningJournal(dbContext, accountId);
        await Assert.That(openingJournal).IsNull();

        var account = await GetAccountFromList(currentUser, accountId);
        await Assert.That(account.OpeningBalance).IsNull();
        await Assert.That(account.OpeningBalanceDate).IsNull();
        await Assert.That(account.CurrentBalance).IsEqualTo(0);
    }

    private static async Task Upsert(
        ICommandHandler<UpsertAccountCommandRequest, UpsertAccountCommandResponse> handler,
        ICurrentUserContext currentUser,
        Guid accountId,
        long openingBalance,
        DateOnly openingDate,
        string name)
    {
        var response = await handler.Handle(
            new UpsertAccountCommandRequest
            {
                UpsertAccountDto = new UpsertAccountDto(
                    accountId,
                    name,
                    null,
                    string.Empty,
                    AccountTypeConstants.Asset,
                    CurrencyConstants.NZD,
                    openingBalance,
                    openingDate)
            },
            currentUser,
            CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    private async Task SetFinanceRangeIncluding(params DateOnly[] dates)
    {
        var financeUserContext = GetRequiredService<IFinanceUserContext>();
        await financeUserContext.LoadAsync(CancellationToken.None);
        var min = dates.Min();
        var max = dates.Max();
        await financeUserContext.SetAsync(min.AddYears(-1), max.AddYears(1), CancellationToken.None);
    }

    private static async Task<TransactionJournal?> GetOpeningJournal(IDbContext<RomeDbContext> dbContext, Guid accountId) =>
        await dbContext
            .Set<TransactionJournal>()
            .AsNoTracking()
            .Include(_ => _.Transactions)
            .FirstOrDefaultAsync(_ =>
                _.TransactionTypeId == TransactionTypeConstants.OpeningBalance &&
                _.Transactions.Any(t => t.AccountId == accountId),
                CancellationToken.None);

    private async Task<AccountDto> GetAccountFromList(ICurrentUserContext currentUser, Guid accountId)
    {
        var accountListHandler = GetRequiredService<IQueryHandler<AccountListQueryRequest, AccountListQueryResponse>>();
        var accountList = await accountListHandler.Handle(
            new AccountListQueryRequest { AccountType = null },
            currentUser,
            CancellationToken.None);

        return accountList.Value.Single(_ => _.Id == accountId);
    }
}
