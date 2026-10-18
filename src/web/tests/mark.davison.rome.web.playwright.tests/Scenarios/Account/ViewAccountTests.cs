namespace mark.davison.rome.web.playwright.tests.Scenarios.Account;

public sealed class ViewAccountTests : RomeBaseTest
{
    [Test]
    public async Task MultiSplitDeposit_ShowsOrderedRegisterOnSourceAccount()
    {
        var splitGroupTitle = $"E2E multi-split {Guid.NewGuid():N}";
        var everydayLeg = $"{splitGroupTitle}-everyday";
        var savingsLeg = $"{splitGroupTitle}-savings";

        var createTransaction = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        await createTransaction
            .Submit(
                new NewTransactionGroupInfo(
                    splitGroupTitle,
                    [
                        new NewTransactionSplitInfo(
                            everydayLeg,
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingEverydayAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            100.0M),
                        new NewTransactionSplitInfo(
                            savingsLeg,
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
            .ThenAsync(_ => _.ExpectRegisterTextsInOrder(splitGroupTitle, everydayLeg));

        var savingsAccount = await ViewAccountPage.OpenAssetAccountByName(
            CurrentPage,
            AppSettings,
            AccountConstants.ExistingSavingsAccountName);

        await savingsAccount
            .ExpectAccountName(AccountConstants.ExistingSavingsAccountName)
            .ThenAsync(_ => _.ExpectRegisterTextsInOrder(splitGroupTitle, savingsLeg));
    }
}
