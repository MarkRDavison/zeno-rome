namespace mark.davison.rome.web.playwright.app.Pages;

public sealed class ViewTransactionPage : RomeBasePage
{
    private readonly string _title;

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
}
