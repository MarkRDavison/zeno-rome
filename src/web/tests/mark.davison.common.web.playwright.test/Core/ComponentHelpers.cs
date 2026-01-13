using Microsoft.Playwright;

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

        await control.ClickAsync();

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
        var control = locator.GetByLabel(label);

        await control.ClickAsync();

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

    public static async Task SelectDate(IPage page, string label, DateOnly value)
    {
        var datePicker = page.GetByLabel(label, new()
        {
            Exact = true
        });

        var id = await datePicker.GetAttributeAsync("id");

        await page.GetByRole(AriaRole.Button, new()
        {
            Name = label
        }).ClickAsync();

        var popup = page.Locator($".mud-popover-provider #{id}");

        var yearButton = popup.Locator(".mud-button-year");

        await yearButton.ClickAsync();

        var pickerIdYear = await popup.Locator(".mud-picker-year").First.GetAttributeAsync("id");

        if (string.IsNullOrEmpty(pickerIdYear) || pickerIdYear.Length < 4)
        {
            throw new InvalidOperationException("Could not find the picker year id");
        }

        var pickerId = pickerIdYear[..^4];

        var expectedId = pickerId + value.ToString("yyyy");

        await popup.Locator($"div[id=\"{expectedId}\"]").ClickAsync();

        var month = value.ToString("MMMM");

        await popup.GetByLabel(month[..3]).ClickAsync();

        var day = value.Day.ToString();

        await popup.GetByText(day, new LocatorGetByTextOptions
        {
            Exact = true
        }).ClickAsync();
    }
    public static async Task SelectDate(IPage page, ILocator locator, string label, DateOnly value)
    {
        var datePicker = locator.GetByLabel(label, new()
        {
            Exact = true
        });

        var id = await datePicker.GetAttributeAsync("id");

        await locator.GetByRole(AriaRole.Button, new()
        {
            Name = label
        }).ClickAsync();

        var popup = page.Locator($".mud-popover-provider #{id}");

        var yearButton = popup.Locator(".mud-button-year");

        await yearButton.ClickAsync();

        var pickerIdYear = await popup.Locator(".mud-picker-year").First.GetAttributeAsync("id");

        if (string.IsNullOrEmpty(pickerIdYear) || pickerIdYear.Length < 4)
        {
            throw new InvalidOperationException("Could not find the picker year id");
        }

        var pickerId = pickerIdYear[..^4];

        var expectedId = pickerId + value.ToString("yyyy");

        await popup.Locator($"div[id=\"{expectedId}\"]").ClickAsync();

        var month = value.ToString("MMMM");

        await popup.GetByLabel(month[..3]).ClickAsync();

        var day = value.Day.ToString();

        await popup.GetByText(day, new LocatorGetByTextOptions
        {
            Exact = true
        }).ClickAsync();
    }
}
