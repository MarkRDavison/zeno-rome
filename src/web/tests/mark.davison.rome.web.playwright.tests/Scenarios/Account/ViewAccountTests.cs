namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;

public sealed class ViewAccountTests : RomeBaseTest
{
    [Test]
    public async Task MultiSplitDeposit_ShowsOrderedRegisterOnSourceAccount()
    {
        var createTransaction = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        await createTransaction
            .Submit(
                new NewTransactionGroupInfo(
                    "Split transaction",
                    [
                        new NewTransactionSplitInfo(
                            "split #1",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingEverydayAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            100.0M),
                        new NewTransactionSplitInfo(
                            "split #2",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingSavingsAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            80.0M)
                    ]));

        var viewAccount = await ViewAccountPage.OpenAssetAccountByName(
            CurrentPage,
            AppSettings,
            AccountConstants.ExistingEverydayAccountName);

        await viewAccount
            .ExpectAccountName(AccountConstants.ExistingEverydayAccountName)
            .ThenAsync(_ => _.ExpectRegisterLeadingRows("Split transaction", "split #1"));

        var savingsAccount = await ViewAccountPage.OpenAssetAccountByName(
            CurrentPage,
            AppSettings,
            AccountConstants.ExistingSavingsAccountName);

        await savingsAccount
            .ExpectAccountName(AccountConstants.ExistingSavingsAccountName)
            .ThenAsync(_ => _.ExpectRegisterLeadingRows("Split transaction", "split #2"));
    }
}
