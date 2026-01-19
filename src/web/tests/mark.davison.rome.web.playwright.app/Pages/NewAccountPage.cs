namespace mark.davison.rome.web.playwright.app.Pages;

public sealed record NewAccountInfo(string Name, string AccountNumber, string Currency);

public sealed class NewAccountPage : RomeBasePage
{
    private readonly AccountType _accountType;

    private const string NameLabel = "Name";
    private const string AccountNumberLabel = "Account number";
    private const string CurrencyLabel = "Currency";
    private const string CreateAccountButton = "Create account";

    public NewAccountPage(
        IPage page,
        PlaywrightAppSettings appSettings,
        AccountType accountType
    ) : base(
        page,
        appSettings)
    {
        _accountType = accountType;
    }

    public async Task<ViewAccountPage> Submit(NewAccountInfo info)
    {
        await FillField(this, NameLabel, info.Name);
        await FillField(this, AccountNumberLabel, info.AccountNumber);
        await SelectField(this, CurrencyLabel, info.Currency);
        return await Submit();
    }

    public async Task<ViewAccountPage> Submit()
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = CreateAccountButton
        }).ClickAsync();

        return await ViewAccountPage.Goto(Page, AppSettings);
    }
}
