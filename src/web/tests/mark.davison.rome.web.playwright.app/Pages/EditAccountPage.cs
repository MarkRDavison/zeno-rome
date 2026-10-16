namespace mark.davison.rome.web.playwright.app.Pages;

public sealed class EditAccountPage : RomeBasePage
{
    private const string SaveAccountButton = "Save account";

    private EditAccountPage(IPage page, PlaywrightAppSettings appSettings) : base(page, appSettings)
    {
    }

    public static async Task<EditAccountPage> Goto(IPage page, PlaywrightAppSettings appSettings, Guid accountId)
    {
        await page.GotoAsync($"{appSettings.WEB_ORIGIN}/account/{accountId}/edit");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = SaveAccountButton }).WaitForAsync();
        return new EditAccountPage(page, appSettings);
    }

    public async Task<ViewAccountPage> SaveAccountName(string name)
    {
        await FillField(this, "Name", name);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = SaveAccountButton }).ClickAsync();
        return await ViewAccountPage.Goto(Page, AppSettings);
    }
}
