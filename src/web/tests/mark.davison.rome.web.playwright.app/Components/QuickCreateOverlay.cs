namespace mark.davison.rome.web.playwright.app.Components;

public sealed class QuickCreateOverlay
{
    private readonly IPage _page;
    private readonly PlaywrightAppSettings _appSettings;

    private QuickCreateOverlay(IPage page, PlaywrightAppSettings appSettings)
    {
        _page = page;
        _appSettings = appSettings;
    }

    public static async Task<QuickCreateOverlay> Open(IPage page, PlaywrightAppSettings appSettings)
    {
        var overlay = new QuickCreateOverlay(page, appSettings);

        await page.GetByTestId(DataTestIds.QuickCreate).ClickAsync();

        return overlay;
    }

    public async Task<NewAccountPage> CreateAccount(AccountType accountType)
    {
        var heading = $"New {accountType.ToString().ToLower()} account";

        await _page.GetByText(heading).ClickAsync();

        return new NewAccountPage(_page, _appSettings, accountType);
    }
}
