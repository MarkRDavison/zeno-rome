namespace mark.davison.rome.web.playwright.app.Pages;

public sealed class ViewTransactionPage : RomeBasePage
{
    private readonly string _title;

    public sealed record SplitInfo(string Description, string SourceAccount, string DestinationAccount, decimal Amount);

    private ViewTransactionPage(IPage page, PlaywrightAppSettings appSettings, string title) : base(page, appSettings)
    {
        _title = title;
    }

    public static async Task<ViewTransactionPage> Goto(IPage page, PlaywrightAppSettings appSettings)
    {
        var titleLocator = page.GetByTestId(DataTestIds.ViewTransactionTitle);

        await titleLocator.WaitForAsync();

        var title = await titleLocator.InnerTextAsync();

        var viewTransactionPage = new ViewTransactionPage(page, appSettings, title);

        return viewTransactionPage;
    }

    public async Task<ViewTransactionPage> ExpectTitle(string title)
    {
        await Assert.That(_title).IsEqualTo(title);

        return this;
    }

    public async Task<ViewTransactionPage> ExpectTransactionInfo(TransactionType type, string? description, DateOnly? date)
    {
        var transactionInfoCard = Page.Locator(".transaction-information-card");

        await transactionInfoCard.IsVisibleAsync();

        var transactionType = await transactionInfoCard.GetByTestId(DataTestIds.ViewTransactionType).InnerTextAsync();

        await Assert.That(transactionType).IsEqualTo(type.ToString());

        if (!string.IsNullOrEmpty(description))
        {
            var transactionDescription = await transactionInfoCard.GetByTestId(DataTestIds.ViewTransactionDescription).InnerTextAsync();

            await Assert.That(transactionDescription).IsEqualTo(description);
        }

        if (date is not null)
        {
            var transactionDate = await transactionInfoCard.GetByTestId(DataTestIds.ViewTransactionDate).InnerTextAsync();

            await Assert.That(transactionDate).IsEqualTo(date.Value.ToString("MMMM d, yyyy"));
        }

        return this;
    }

    public async Task<ViewTransactionPage> ExpectMetaInfo(IEnumerable<string> sourceAccounts, decimal total)
    {
        var transactionInfoCard = Page.Locator(".meta-information-card");

        await transactionInfoCard.IsVisibleAsync();

        var sourceAccountText = await transactionInfoCard.GetByTestId(DataTestIds.SourceAccounts).AllInnerTextsAsync();

        foreach (var sourceAccountName in sourceAccounts)
        {
            await Assert.That(sourceAccountText).Contains(sourceAccountName);
        }

        var totalAmount = await transactionInfoCard.GetByTestId(DataTestIds.ViewTransactionTotalAmount).InnerTextAsync();

        await Assert.That(totalAmount).Contains(total.ToString());

        return this;
    }

    public async Task<ViewTransactionPage> ExpectSplitInfo(IEnumerable<SplitInfo> splits)
    {
        var transactionCards = await Page.Locator(".transaction-card").AllAsync();

        await Assert.That(transactionCards).Count().IsEqualTo(splits.Count());

        for (int i = 0; i < transactionCards.Count; ++i)
        {
            var transactionCard = transactionCards[i];
            var split = splits.ElementAt(i);

            var cardHeader = await transactionCard.Locator(".mud-card-header-content").InnerTextAsync();

            await Assert.That(cardHeader).IsEqualTo(split.Description);

            var cardContent = await transactionCard.Locator(".mud-card-content").InnerTextAsync();

            var cardContents = cardContent.Split('→');

            await Assert.That(cardContents).Count().IsEqualTo(3);

            await Assert.That(cardContents[0]).Contains(split.SourceAccount);
            await Assert.That(cardContents[1]).Contains(split.Amount.ToString());
            await Assert.That(cardContents[2]).Contains(split.DestinationAccount);
        }

        return this;
    }
}
