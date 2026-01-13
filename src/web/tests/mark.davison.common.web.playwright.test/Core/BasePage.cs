namespace mark.davison.common.web.playwright.test.Core;

public abstract class BasePage<TAppSettings> where TAppSettings : class
{
    protected readonly IPage Page;
    protected readonly TAppSettings AppSettings;

    protected BasePage(IPage page, TAppSettings appSettings)
    {
        Page = page;
        AppSettings = appSettings;
    }

    public virtual Task<TPage> GoToPage<TPage>() where TPage : BasePage<TAppSettings>
    {
        throw new NotImplementedException();
    }

    protected async Task<TPage> FillField<TPage>(TPage page, string label, string value) where TPage : BasePage<TAppSettings>
    {
        await Page.GetByLabel(label).FillAsync(value);
        return page;
    }
    protected async Task<TPage> FillField<TPage>(TPage page, ILocator locator, string label, string value) where TPage : BasePage<TAppSettings>
    {
        await locator.GetByLabel(label).FillAsync(value);
        return page;
    }
    protected async Task<TPage> FillField<TPage>(TPage page, string label, decimal value) where TPage : BasePage<TAppSettings>
    {
        return await FillField<TPage>(page, label, value.ToString());
    }
    protected async Task<TPage> FillField<TPage>(TPage page, ILocator locator, string label, decimal value) where TPage : BasePage<TAppSettings>
    {
        return await FillField<TPage>(page, locator, label, value.ToString());
    }
    protected async Task<TPage> FillField<TPage>(TPage page, string label, DateOnly value) where TPage : BasePage<TAppSettings>
    {
        await ComponentHelpers.SelectDate(Page, label, value);
        return page;
    }
    protected async Task<TPage> FillField<TPage>(TPage page, ILocator locator, string label, DateOnly value) where TPage : BasePage<TAppSettings>
    {
        await ComponentHelpers.SelectDate(Page, locator, label, value);
        return page;
    }
    protected async Task<TPage> SelectField<TPage>(TPage page, string label, string value) where TPage : BasePage<TAppSettings>
    {
        await ComponentHelpers.SelectAsync(Page, label, value);
        return page;
    }
    protected async Task<TPage> SelectField<TPage>(TPage page, ILocator locator, string label, string value) where TPage : BasePage<TAppSettings>
    {
        await ComponentHelpers.SelectAsync(Page, locator, label, value);
        return page;
    }
}