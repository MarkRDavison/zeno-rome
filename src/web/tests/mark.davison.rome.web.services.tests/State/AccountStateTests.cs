namespace mark.davison.rome.web.services.tests.State;

public sealed class AccountStateTests
{
    [Test]
    public async Task FetchAccount_UsesAccountByIdQuery()
    {
        var accountId = Guid.NewGuid();
        var dto = new AccountDto(
            accountId,
            "Savings",
            AccountTypeConstants.Asset,
            "1",
            0,
            true,
            DateTimeOffset.UtcNow,
            0,
            CurrencyConstants.NZD,
            null,
            null,
            null);

        var client = new Mock<IClientHttpRepository>();
        client
            .Setup(_ => _.Get<AccountByIdQueryRequest, AccountByIdQueryResponse>(
                It.Is<AccountByIdQueryRequest>(r => r.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountByIdQueryResponse { Value = dto });

        var state = new AccountState(client.Object);

        await state.FetchAccount(accountId);

        await Assert.That(state.Loaded).IsTrue();
        await Assert.That(state.Accounts).Count().IsEqualTo(1);
        await Assert.That(state.Accounts[0].Id).IsEqualTo(accountId);
    }

    [Test]
    public async Task MergeAccounts_KeepsDistinctAccounts()
    {
        var client = new Mock<IClientHttpRepository>();
        var state = new AccountState(client.Object);
        var first = new AccountDto(Guid.NewGuid(), "A", AccountTypeConstants.Asset, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, CurrencyConstants.NZD, null, null, null);
        var second = new AccountDto(Guid.NewGuid(), "B", AccountTypeConstants.Expense, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, CurrencyConstants.NZD, null, null, null);

        state.SetState([first]);
        state.MergeAccounts([second]);

        await Assert.That(state.Accounts).Count().IsEqualTo(2);
    }
}
