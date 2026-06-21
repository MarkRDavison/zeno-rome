namespace mark.davison.rome.web.playwright.tests.Scenarios.Transaction;

public sealed class AddTransactionTests : RomeBaseTest
{
    [Test]
    public async Task AddingTransactionWithSingleSplitWorks()
    {
        var createTransaction = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        var newTransaction = await createTransaction
            .Submit(
                new NewTransactionGroupInfo(
                    null,
                    [
                        new NewTransactionSplitInfo(
                            "split #1",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingEverydayAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            100.0M)
                    ]));

        await newTransaction
            .ExpectTitle("View Transaction")
            .ThenAsync(_ => _.ExpectTransactionInfo(TransactionType.Deposit, null, DateOnly.FromDateTime(DateTime.Today)))
            .ThenAsync(_ => _.ExpectMetaInfo([AccountConstants.ExistingWorkAccountName], 100.0M))
            .ThenAsync(_ => _.ExpectSplitInfo([
                new("split #1", AccountConstants.ExistingWorkAccountName, AccountConstants.ExistingEverydayAccountName, 100.0M)
            ]));
    }

    [Test]
    public async Task AddingTransactionWithMultipleSplitsWorks()
    {
        var createTransaction = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        var newTransaction = await createTransaction
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

        await newTransaction
            .ExpectTitle("View Transaction")
            .ThenAsync(_ => _.ExpectTransactionInfo(TransactionType.Deposit, "Split transaction", DateOnly.FromDateTime(DateTime.Today)))
            .ThenAsync(_ => _.ExpectMetaInfo([AccountConstants.ExistingWorkAccountName], 180.0M))
            .ThenAsync(_ => _.ExpectSplitInfo([
                new("split #1", AccountConstants.ExistingWorkAccountName, AccountConstants.ExistingEverydayAccountName, 100.0M),
                new("split #2", AccountConstants.ExistingWorkAccountName, AccountConstants.ExistingSavingsAccountName, 80.0M),
            ]));
    }
}
