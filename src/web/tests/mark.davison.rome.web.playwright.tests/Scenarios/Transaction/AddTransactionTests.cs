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

        await newTransaction.ExpectTitle("View Transaction");
    }
}
