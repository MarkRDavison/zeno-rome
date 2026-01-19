namespace mark.davison.rome.web.playwright.app.Pages;

public sealed class ViewAccountPage : RomeBasePage
{
    private readonly string _title;

    private ViewAccountPage(IPage page, PlaywrightAppSettings appSettings, string title) : base(page, appSettings)
    {
        _title = title;
    }

    public static async Task<ViewAccountPage> Goto(IPage page, PlaywrightAppSettings appSettings)
    {
        var titleLocator = page.GetByTestId(DataTestIds.ViewAccountTitle);

        await titleLocator.WaitForAsync();

        var title = await titleLocator.InnerTextAsync();

        var viewAccountPage = new ViewAccountPage(page, appSettings, title);

        return viewAccountPage;
    }

    public async Task<ViewAccountPage> ExpectAccountName(string name)
    {
        await Assert.That(_title).IsEqualTo(name);

        return this;
    }
}
