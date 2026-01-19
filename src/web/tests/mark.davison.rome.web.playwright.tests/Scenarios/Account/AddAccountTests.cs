namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;


public sealed class AddAccountTests : RomeBaseTest
{
    [Arguments(AccountType.Asset)]
    [Arguments(AccountType.Expense)]
    [Arguments(AccountType.Revenue)]
    [Test]
    public async Task AddAccountWorks(AccountType accountType)
    {
        var accountName = MakeAccountName();
        var accountNumber = MakeAccountNumber();

        var createAccount = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateAccount(accountType));

        var newAccount = await createAccount
            .Submit(new NewAccountInfo(
                accountName,
                accountNumber,
                CurrencyConstants.NZD));

        await newAccount.ExpectAccountName(accountName);

    }
}
