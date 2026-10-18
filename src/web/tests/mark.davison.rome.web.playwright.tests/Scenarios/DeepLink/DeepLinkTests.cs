namespace mark.davison.rome.web.playwright.tests.Scenarios.DeepLink;

public sealed class DeepLinkTests : RomeBaseTest
{
    [Test]
    public async Task ViewAccount_DirectUrlAndReload_ShowsAccount()
    {
        var accountName = MakeAccountName();

        var createAccount = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateAccount(AccountType.Asset));

        await createAccount.Submit(new NewAccountInfo(
            accountName,
            MakeAccountNumber(),
            CurrencyConstants.NZD));

        var accountId = ViewAccountPage.ParseAccountIdFromUrl(CurrentPage.Url);

        var viewAccount = await ViewAccountPage.GotoAccount(CurrentPage, AppSettings, accountId);
        await viewAccount.ExpectAccountName(accountName);

        await CurrentPage.ReloadAsync();
        viewAccount = await ViewAccountPage.Goto(CurrentPage, AppSettings);
        await viewAccount.ExpectAccountName(accountName);
    }

    [Test]
    public async Task ViewTransaction_DirectUrlAndReload_ShowsTransaction()
    {
        var createTransaction = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        await createTransaction.Submit(
            new NewTransactionGroupInfo(
                null,
                [
                    new NewTransactionSplitInfo(
                        "split #1",
                        AccountConstants.ExistingWorkAccountName,
                        AccountConstants.ExistingEverydayAccountName,
                        DateOnly.FromDateTime(DateTime.Today),
                        50.0M)
                ]));

        var groupId = ViewTransactionPage.ParseTransactionGroupIdFromUrl(CurrentPage.Url);

        var viewTransaction = await ViewTransactionPage.GotoTransaction(CurrentPage, AppSettings, groupId);
        await viewTransaction.ExpectTitle("View Transaction");

        await CurrentPage.ReloadAsync();
        viewTransaction = await ViewTransactionPage.Goto(CurrentPage, AppSettings);
        await viewTransaction.ExpectTitle("View Transaction");
    }
}
