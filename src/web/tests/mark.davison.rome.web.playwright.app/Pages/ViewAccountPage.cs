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

    public static async Task<ViewAccountPage> OpenAssetAccountByName(IPage page, PlaywrightAppSettings appSettings, string accountName)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Asset accounts" }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = accountName, Exact = true }).ClickAsync();
        return await Goto(page, appSettings);
    }

    public async Task<ViewAccountPage> ExpectAccountName(string name)
    {
        await Assert.That(_title).IsEqualTo(name);

        return this;
    }

    public async Task<ViewAccountPage> ExpectCurrentBalanceContains(string amountFragment)
    {
        var balance = await Page.GetByTestId(DataTestIds.ViewAccountCurrentBalance).InnerTextAsync();
        await Assert.That(balance).Contains(amountFragment);

        return this;
    }

    /// <summary>
    /// Asserts the first N register rows in visual order (top of grid).
    /// </summary>
    public async Task<ViewAccountPage> ExpectRegisterLeadingRows(params string[] expectedTextsInOrder)
    {
        for (var i = 0; i < expectedTextsInOrder.Length; i++)
        {
            var row = Page.GetByTestId($"{DataTestIds.ViewAccountRegisterRowPrefix}{i}");
            await row.WaitForAsync();
            var text = await row.InnerTextAsync();
            await Assert.That(text).Contains(expectedTextsInOrder[i]);
        }

        return this;
    }
}
