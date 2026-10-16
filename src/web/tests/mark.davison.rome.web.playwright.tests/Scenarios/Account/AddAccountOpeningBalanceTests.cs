namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;

public sealed class AddAccountOpeningBalanceTests : RomeBaseTest
{
    [Test]
    [Arguments(AccountType.Asset)]
    [Arguments(AccountType.Expense)]
    public async Task AddAccountWithOpeningBalance_ShowsBalanceAndRegister(AccountType accountType)
    {
        var accountName = MakeAccountName();
        var accountNumber = MakeAccountNumber();
        const string openingAmountInput = "2500.00";
        const string expectedBalanceFragment = "2,500.00";
        var openingDate = DateOnly.FromDateTime(DateTime.Today);

        var createAccount = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateAccount(accountType));

        var viewAccount = await createAccount.Submit(new NewAccountInfo(
            accountName,
            accountNumber,
            CurrencyConstants.NZD,
            openingAmountInput,
            openingDate));

        await viewAccount.ExpectAccountName(accountName);
        await viewAccount.ExpectCurrentBalanceContains(expectedBalanceFragment);
        await viewAccount.ExpectRegisterLeadingRows("Opening balance");
    }
}
