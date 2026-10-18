using System.Globalization;

namespace mark.davison.common.web.playwright.test.Core;

public static class ComponentHelpers
{
    // TODO: Consolidate Page/Locator versions
    public static async Task SetAutoComplete(IPage page, string label, string value)
    {
        var parentFandomAutocomplete = page.GetByLabel(label);

        await parentFandomAutocomplete.ClickAsync();
        await parentFandomAutocomplete.PressSequentiallyAsync(value);

        var popupOptions = await page.Locator(".mud-popover-open p").AllAsync();

        foreach (var option in popupOptions)
        {
            var text = await option.TextContentAsync();

            if (text == value)
            {
                await option.ClickAsync();
            }
        }
    }

    public static async Task SetAutoComplete(IPage page, ILocator locator, string label, string value)
    {
        var parentFandomAutocomplete = locator.GetByLabel(label);

        await parentFandomAutocomplete.ClickAsync();
        await parentFandomAutocomplete.PressSequentiallyAsync(value);

        var popupOptions = await page.Locator(".mud-popover-open p").AllAsync();

        foreach (var option in popupOptions)
        {
            var text = await option.TextContentAsync();

            if (text == value)
            {
                await option.ClickAsync();
            }
        }
    }

    public static async Task SelectAsync(IPage page, string label, string value)
    {
        var control = page.GetByLabel(label);

        await OpenMudSelectAsync(control);

        var popupOptions = await page.Locator(".mud-popover-open p").AllAsync();

        foreach (var option in popupOptions)
        {
            var text = await option.TextContentAsync();

            if (text == value)
            {
                await option.ClickAsync();
                return;
            }
        }

        Assert.Fail(string.Format("Could not find '{0}' for '{1}' control.", value, label));
    }

    public static async Task SelectAsync(IPage page, ILocator locator, string label, string value)
    {
        var control = locator.GetByLabel(label, new LocatorGetByLabelOptions { Exact = true });

        await OpenMudSelectAsync(control);

        var popupOptions = await page.Locator(".mud-popover-open p").AllAsync();

        foreach (var option in popupOptions)
        {
            var text = await option.TextContentAsync();

            if (text == value)
            {
                await option.ClickAsync();
                return;
            }
        }

        Assert.Fail(string.Format("Could not find '{0}' for '{1}' control.", value, label));
    }

    public static async Task SelectMultiAsync(IPage page, ILocator locator, string label, IEnumerable<string> values)
    {
        var control = locator.GetByLabel(label);

        await control.ClickAsync();

        foreach (var value in values)
        {
            var matched = false;
            var popupOptions = await page.Locator(".mud-popover-open .mud-list-item").AllAsync();

            foreach (var option in popupOptions)
            {
                var text = (await option.TextContentAsync())?.Trim();

                if (text == value)
                {
                    await option.ClickAsync();
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                Assert.Fail(string.Format("Could not find '{0}' for multi-select '{1}' control.", value, label));
            }
        }

        await page.Keyboard.PressAsync("Escape");
    }

    public static async Task SelectDate(IPage page, string label, DateOnly value)
    {
        var datePicker = page.GetByLabel(label, new PageGetByLabelOptions { Exact = true });
        await SelectMudDatePickerAsync(page, datePicker, value);
    }

    public static async Task SelectDate(IPage page, ILocator locator, string label, DateOnly value)
    {
        var datePicker = locator.GetByLabel(label, new LocatorGetByLabelOptions { Exact = true });
        await SelectMudDatePickerAsync(page, datePicker, value);
    }

    private static async Task OpenMudSelectAsync(ILocator labeledControl)
    {
        if (await labeledControl.IsVisibleAsync())
        {
            await labeledControl.ClickAsync();
            return;
        }

        var controlScope = labeledControl.Locator("xpath=ancestor::div[contains(@class,'mud-input-control')]");
        await controlScope.GetByRole(AriaRole.Button).First.ClickAsync();
    }

    private static async Task SelectMudDatePickerAsync(IPage page, ILocator datePickerInput, DateOnly value)
    {
        var id = await datePickerInput.GetAttributeAsync("id");
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException("MudDatePicker input must have an id attribute for popover selection.");
        }

        var controlScope = datePickerInput.Locator("xpath=ancestor::div[contains(@class,'mud-input-control')]");
        await controlScope.GetByRole(AriaRole.Button).ClickAsync();

        var popup = page.Locator($".mud-popover-provider #{id}");

        await popup.Locator(".mud-button-year").ClickAsync();

        var pickerIdYear = await popup.Locator(".mud-picker-year").First.GetAttributeAsync("id");
        if (string.IsNullOrEmpty(pickerIdYear) || pickerIdYear.Length < 4)
        {
            throw new InvalidOperationException("Could not find the picker year id");
        }

        var pickerId = pickerIdYear[..^4];
        var expectedId = pickerId + value.ToString("yyyy");
        await popup.Locator($"div[id=\"{expectedId}\"]").ClickAsync();

        var months = popup.Locator(".mud-picker-month-container button");
        await months.Nth(value.Month - 1).ClickAsync();

        await ClickCalendarDayAsync(popup, value);
    }

    public static async Task SelectDateInOpenMudPopover(ILocator popover, DateOnly date)
    {
        await NavigateMudPopoverToYearMonthAsync(popover, date);
        await ClickCalendarDayAsync(popover, date);
    }

    private static async Task NavigateMudPopoverToYearMonthAsync(ILocator popover, DateOnly date)
    {
        await popover.Locator(".mud-button-year").ClickAsync();

        var pickerIdYear = await popover.Locator(".mud-picker-year").First.GetAttributeAsync("id");
        if (string.IsNullOrEmpty(pickerIdYear) || pickerIdYear.Length < 4)
        {
            throw new InvalidOperationException("Could not find the picker year id");
        }

        var pickerId = pickerIdYear[..^4];
        var expectedId = pickerId + date.ToString("yyyy", CultureInfo.InvariantCulture);
        await popover.Locator($"div[id=\"{expectedId}\"]").ClickAsync();

        var months = popover.Locator(".mud-picker-month-container button");
        await months.Nth(date.Month - 1).ClickAsync();
    }

    private static async Task ClickCalendarDayAsync(ILocator popover, DateOnly date)
    {
        var monthName = date.ToString("MMMM", CultureInfo.InvariantCulture);
        var day = date.Day.ToString(CultureInfo.InvariantCulture);

        var header = popover
            .Locator(".mud-picker-calendar-header")
            .Filter(new LocatorFilterOptions { HasTextString = monthName })
            .Filter(new LocatorFilterOptions { HasTextString = date.Year.ToString(CultureInfo.InvariantCulture) });

        var calendar = header
            .First
            .Locator("xpath=following-sibling::div[contains(@class,'mud-picker-calendar-transition')]");

        await calendar
            .Locator("button.mud-picker-calendar-day:not(.mud-hidden)")
            .GetByText(day, new LocatorGetByTextOptions { Exact = true })
            .ClickAsync();
    }
}
