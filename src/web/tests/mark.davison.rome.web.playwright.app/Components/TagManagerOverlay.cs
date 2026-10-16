namespace mark.davison.rome.web.playwright.app.Components;

public sealed class TagManagerOverlay : RomeBasePage
{
    private const string NewTagLabel = "New tag";
    private const string AddButtonLabel = "Add";

    private TagManagerOverlay(IPage page, PlaywrightAppSettings appSettings) : base(page, appSettings)
    {
    }

    public static async Task<TagManagerOverlay> OpenFromNav(IPage page, PlaywrightAppSettings appSettings)
    {
        await page.GetByTestId(DataTestIds.NavTags).ClickAsync();

        var overlay = new TagManagerOverlay(page, appSettings);
        await overlay.WaitForReadyAsync();

        return overlay;
    }

    public static async Task<TagManagerOverlay> OpenFromManageTagsLink(IPage page, PlaywrightAppSettings appSettings)
    {
        await page.GetByTestId(DataTestIds.ManageTagsLink).ClickAsync();

        var overlay = new TagManagerOverlay(page, appSettings);
        await overlay.WaitForReadyAsync();

        return overlay;
    }

    public async Task CloseAsync()
    {
        var closeButton = Scope().GetByTestId(DataTestIds.TagClose);
        if (await closeButton.IsVisibleAsync())
        {
            await closeButton.ClickAsync();
        }
        else
        {
            await Page.Keyboard.PressAsync("Escape");
        }

        await Assertions.Expect(Page.GetByTestId(DataTestIds.TagListTitle)).ToBeHiddenAsync();
    }

    private ILocator Scope() =>
        Page.Locator(".mud-dialog, main").Filter(new LocatorFilterOptions
        {
            Has = Page.GetByTestId(DataTestIds.TagListTitle)
        }).First;

    private async Task WaitForReadyAsync()
    {
        await Page.GetByTestId(DataTestIds.TagListTitle).WaitForAsync();
        await Scope().GetByTestId(DataTestIds.TagList).WaitForAsync();
    }

    public async Task<TagManagerOverlay> AddTag(string name)
    {
        var scope = Scope();

        await FillField(this, scope, NewTagLabel, name);

        await Scope().GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            Name = AddButtonLabel
        }).ClickAsync();

        return this;
    }

    public async Task<TagManagerOverlay> ExpectTagListed(string name)
    {
        var list = Scope().GetByTestId(DataTestIds.TagList);

        await Assertions.Expect(list.GetByText(name, new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        return this;
    }
}
