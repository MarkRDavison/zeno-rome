using mark.davison.rome.web.playwright.app.Helpers;

namespace mark.davison.rome.web.playwright.tests.Scenarios.Tags;

public sealed class TagManagerTests : RomeBaseTest
{
    [Test]
    public async Task AddTag_FromNav_ShowsTagInList()
    {
        var tagName = MakeTagName();

        var overlay = await TagManagerOverlay.OpenFromNav(CurrentPage, AppSettings);

        await overlay.AddTag(tagName);
        await overlay.ExpectTagListed(tagName);
    }

    [Test]
    public async Task AddTag_FromManageTagsOnNewTransaction_ShowsTagInList()
    {
        var tagName = MakeTagName();

        await Dashboard
            .OpenQuickCreate()
            .ThenAsync(_ => _.CreateTransaction(TransactionType.Deposit));

        var overlay = await TagManagerOverlay.OpenFromManageTagsLink(CurrentPage, AppSettings);

        await overlay.AddTag(tagName);
        await overlay.ExpectTagListed(tagName);
        await overlay.CloseAsync();
    }
}
