namespace mark.davison.rome.api.tests.Scenarios.AccountById;

public sealed class AccountByIdIntegrationTests : ApiIntegrationTestBase
{
    [Test]
    public async Task AccountById_ReturnsCreatedAccount()
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var financeUserContext = GetRequiredService<IFinanceUserContext>();
        await financeUserContext.LoadAsync(CancellationToken.None);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await financeUserContext.SetAsync(today.AddYears(-1), today.AddYears(1), CancellationToken.None);

        var accountId = Guid.NewGuid();
        var upsertHandler = GetRequiredService<ICommandHandler<UpsertAccountCommandRequest, UpsertAccountCommandResponse>>();
        var upsertResponse = await upsertHandler.Handle(
            new UpsertAccountCommandRequest
            {
                UpsertAccountDto = new UpsertAccountDto(
                    accountId,
                    "Deep link account",
                    null,
                    "ACC-99",
                    AccountTypeConstants.Asset,
                    CurrencyConstants.NZD,
                    null,
                    null)
            },
            currentUser,
            CancellationToken.None);

        await Assert.That(upsertResponse.SuccessWithValue).IsTrue();

        var byIdHandler = GetRequiredService<IQueryHandler<AccountByIdQueryRequest, AccountByIdQueryResponse>>();
        var byIdResponse = await byIdHandler.Handle(
            new AccountByIdQueryRequest { AccountId = accountId },
            currentUser,
            CancellationToken.None);

        await Assert.That(byIdResponse.SuccessWithValue).IsTrue();
        await Assert.That(byIdResponse.Value!.Name).IsEqualTo("Deep link account");
        await Assert.That(byIdResponse.Value.AccountNumber).IsEqualTo("ACC-99");
    }

    [Test]
    public async Task AccountById_UnknownId_ReturnsNotFound()
    {
        var currentUser = GetRequiredService<ICurrentUserContext>();
        var handler = GetRequiredService<IQueryHandler<AccountByIdQueryRequest, AccountByIdQueryResponse>>();

        var response = await handler.Handle(
            new AccountByIdQueryRequest { AccountId = Guid.NewGuid() },
            currentUser,
            CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsFalse();
        await Assert.That(response.Errors).Contains("ACCOUNT_NOT_FOUND");
    }
}
