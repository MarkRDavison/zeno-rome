namespace mark.davison.rome.web.playwright.app.Pages;

public abstract class RomeBasePage(IPage page, PlaywrightAppSettings appSettings) : BasePage<PlaywrightAppSettings>(page, appSettings)
{
    public Task<QuickCreateOverlay> OpenQuickCreate() => QuickCreateOverlay.Open(Page, AppSettings);
}
