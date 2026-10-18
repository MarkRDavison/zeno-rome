namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;

public sealed class AccountDateRangeTests : RomeBaseTest
{
    [Test]
    public async Task ChangingDateRange_FiltersRegisterOnAccountView()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var lastMonthMid = today.AddMonths(-1);
        var lastMonthStart = new DateOnly(lastMonthMid.Year, lastMonthMid.Month, 1);
        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

        // Faker names collide with seeded / prior runs; deposits and nav must be unambiguous.
        var accountName = $"M24-e2e-{Guid.NewGuid():N}";
        var accountNumber = MakeAccountNumber();

        const string thisMonthDescription = "M24 this month";
        const string lastMonthDescription = "M24 last month";

        await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateAccount(AccountType.Asset))
            .ThenAsync(_ => _.Submit(new NewAccountInfo(
                accountName,
                accountNumber,
                CurrencyConstants.NZD)));

        var accountId = ViewAccountPage.ParseAccountIdFromUrl(CurrentPage.Url);

        await CreateDepositOnHome(thisMonthDescription, today, 100.0M, accountName);
        await CreateDepositOnHome(lastMonthDescription, lastMonthMid, 50.0M, accountName);

        var viewAccount = await ViewAccountPage.GotoAccount(CurrentPage, AppSettings, accountId);

        await viewAccount
            .ExpectRegisterContains(thisMonthDescription)
            .ThenAsync(_ => _.ExpectRegisterDoesNotContain(lastMonthDescription));

        await AppContextDateRange.On(CurrentPage).SetInclusiveRange(lastMonthStart, lastMonthEnd);

        viewAccount = await ViewAccountPage.GotoAccount(CurrentPage, AppSettings, accountId);
        await viewAccount
            .ExpectRegisterContains(lastMonthDescription)
            .ThenAsync(_ => _.ExpectRegisterDoesNotContain(thisMonthDescription));
    }

    private async Task CreateDepositOnHome(string description, DateOnly date, decimal amount, string destinationAccountName)
    {
        await CurrentPage.GotoAsync(AppSettings.WEB_ORIGIN);

        await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit))
            .ThenAsync(_ => _.Submit(new NewTransactionGroupInfo(
                null,
                [
                    new NewTransactionSplitInfo(
                        description,
                        AccountConstants.ExistingWorkAccountName,
                        destinationAccountName,
                        date,
                        amount)
                ])));
    }
}
