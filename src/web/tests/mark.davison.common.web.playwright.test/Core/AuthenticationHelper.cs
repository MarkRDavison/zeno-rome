namespace mark.davison.common.web.playwright.test.Core;

public partial class AuthenticationHelper
{
    private readonly PlaywrightAppSettings _appSettings;
    private readonly IDataNames _dataNames;

    private const string UsernameLabel = "Username or email";
    private const string Password = "Password";
    private const string ExpectedTitle = "Sign in";

    private int _retryCount = 0;
    private const int MaxRetries = 1;

    public AuthenticationHelper(
        PlaywrightAppSettings appSettings,
        IDataNames dataNames)
    {
        _appSettings = appSettings;
        _dataNames = dataNames;
    }

    public async Task EnsureLoggedIn(IPage page)
    {
        var username = string.Empty;

        try
        {
            username = await page.GetByTestId(_dataNames.Username).TextContentAsync(new LocatorTextContentOptions
            {
                Timeout = 5000.0f // TODO: Config
            });
        }
        catch (TimeoutException)
        {

        }

        if (string.IsNullOrEmpty(username))
        {
            // TODO: Need a way to specify login flow for each provider
            var loginWithProviderLink = page.GetByText($"Login with {_appSettings.AUTH_PROVIDER}");

            try
            {
                await Assertions.Expect(loginWithProviderLink).ToBeVisibleAsync();
            }
            catch (Exception)
            {
                // TODO: More robust logging in/auth state persistence
                if (_retryCount < MaxRetries)
                {
                    _retryCount++;

                    await page.ReloadAsync();

                    await EnsureLoggedIn(page);

                    return;
                }

                throw;
            }

            await loginWithProviderLink.ClickAsync();

            await Assertions.Expect(page).ToHaveTitleAsync(ExpectedTitleRegex(), new PageAssertionsToHaveTitleOptions
            {
                Timeout = 10_000.0f // TODO: Config
            });

            await page.GetByLabel(UsernameLabel).FillAsync(_appSettings.AUTH_USERNAME);
            await page.Locator("#password").FillAsync(_appSettings.AUTH_PASSWORD);

            var button = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
            {
                NameString = "Sign In"
            });

            await button.ClickAsync();
        }

        await Assertions.Expect(page)
            .ToHaveURLAsync(
                new Regex(_appSettings.WEB_ORIGIN),
                new PageAssertionsToHaveURLOptions
                {
                    Timeout = 10_000.0f// TODO: Config
                });
        await Assertions.Expect(page).ToHaveTitleAsync(_appSettings.APP_TITLE);
    }

    [GeneratedRegex(ExpectedTitle, RegexOptions.Compiled)]
    private static partial Regex ExpectedTitleRegex();
}