namespace mark.davison.rome.web.playwright.app.Components;

public sealed class AppContextDateRange
{
    private readonly IPage _page;

    private AppContextDateRange(IPage page) => _page = page;

    public static AppContextDateRange On(IPage page) => new(page);

    public async Task SetInclusiveRange(DateOnly start, DateOnly end)
    {
        await _page.GetByTestId(DataTestIds.AppContextDateRangeButton).ClickAsync();

        var popover = _page.Locator(".mud-popover-open");
        await popover.WaitForAsync();

        await ComponentHelpers.SelectDateInOpenMudPopover(popover, start);
        await ComponentHelpers.SelectDateInOpenMudPopover(popover, end);

        await _page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Matches default app context (current calendar month). Call at test start when prior tests may have changed the range.
    /// </summary>
    public Task ResetToCurrentMonthAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = new DateOnly(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return SetInclusiveRange(start, end);
    }
}
