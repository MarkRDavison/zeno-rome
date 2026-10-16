namespace mark.davison.rome.web.playwright.tests.Scenarios.Transaction;

public sealed class AddTransactionWithTagsTests : RomeBaseTest
{
    [Test]
    public async Task CreateDeposit_WithTags_ShowsTagsOnView()
    {
        var tagName = MakeTagName();
        await SeedTag(tagName);

        var viewPage = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit))
            .ThenAsync(page => page.Submit(
                new NewTransactionGroupInfo(
                    null,
                    [
                        new NewTransactionSplitInfo(
                            "split #1",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingEverydayAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            100.0M,
                            [tagName])
                    ])));

        await viewPage
            .ExpectTitle("View Transaction")
            .ThenAsync(_ => _.ExpectSplitInfo([
                new ViewTransactionPage.SplitInfo(
                    "split #1",
                    AccountConstants.ExistingWorkAccountName,
                    AccountConstants.ExistingEverydayAccountName,
                    100.0M,
                    [tagName])
            ]));
    }

    [Test]
    public async Task CreateDeposit_MultiSplit_DifferentTagsPerSplit()
    {
        var tagOne = MakeTagName();
        var tagTwo = MakeTagName();
        await SeedTag(tagOne);
        await SeedTag(tagTwo);

        var viewPage = await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit))
            .ThenAsync(page => page.Submit(
                new NewTransactionGroupInfo(
                    "Split transaction",
                    [
                        new NewTransactionSplitInfo(
                            "split #1",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingEverydayAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            100.0M,
                            [tagOne]),
                        new NewTransactionSplitInfo(
                            "split #2",
                            AccountConstants.ExistingWorkAccountName,
                            AccountConstants.ExistingSavingsAccountName,
                            DateOnly.FromDateTime(DateTime.Today),
                            80.0M,
                            [tagTwo])
                    ])));

        await viewPage
            .ExpectTitle("View Transaction")
            .ThenAsync(_ => _.ExpectSplitInfo([
                new ViewTransactionPage.SplitInfo(
                    "split #1",
                    AccountConstants.ExistingWorkAccountName,
                    AccountConstants.ExistingEverydayAccountName,
                    100.0M,
                    [tagOne]),
                new ViewTransactionPage.SplitInfo(
                    "split #2",
                    AccountConstants.ExistingWorkAccountName,
                    AccountConstants.ExistingSavingsAccountName,
                    80.0M,
                    [tagTwo])
            ]));
    }

    private async Task SeedTag(string tagName)
    {
        var overlay = await TagManagerOverlay.OpenFromNav(CurrentPage, AppSettings);
        await overlay.AddTag(tagName);
        await overlay.CloseAsync();
    }
}
