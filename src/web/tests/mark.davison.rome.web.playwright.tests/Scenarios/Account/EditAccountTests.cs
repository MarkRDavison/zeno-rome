namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;

public sealed class EditAccountTests : RomeBaseTest
{
    [Test]
    public async Task EditAccountName_ShowsUpdatedNameOnView()
    {
        var accountName = MakeAccountName();
        var updatedName = $"{accountName} updated";

        var createAccount = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateAccount(AccountType.Asset));

        var viewAccount = await createAccount.Submit(new NewAccountInfo(
            accountName,
            MakeAccountNumber(),
            CurrencyConstants.NZD));

        var accountId = ViewAccountPage.ParseAccountIdFromUrl(CurrentPage.Url);

        var editAccount = await EditAccountPage.Goto(CurrentPage, AppSettings, accountId);
        viewAccount = await editAccount.SaveAccountName(updatedName);

        await viewAccount.ExpectAccountName(updatedName);
    }
}
