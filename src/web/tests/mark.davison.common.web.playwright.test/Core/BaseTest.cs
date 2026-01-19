namespace mark.davison.common.web.playwright.test.Core;

public abstract class BaseTest : PageTest, IAsyncDisposable
{
    private static IBrowser? _browser;
    private static IBrowserContext? _context;
    protected readonly Faker _faker = new Faker();
    protected readonly HttpClient _client;
    private const string _authStateFilename = ".auth.json";

    protected BaseTest(IDataNames dataNames)
    {
        _client = new HttpClient();

        if (CreateAppSettings() is { } settings)
        {
            AppSettings = settings;
        }
        else
        {
            return;
        }

        AuthenticationHelper = new AuthenticationHelper(AppSettings, dataNames);
    }

    private static PlaywrightAppSettings? CreateAppSettings()
    {
        var appSettings = new PlaywrightAppSettings();
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .Build();

        var section = config.GetSection(appSettings.Section);

        section.Bind(appSettings);

        if (!appSettings.Validate())
        {
            Console.WriteLine("Invalid app settings");
            return null;
        }

        return appSettings;
    }

    private static string AuthStateFullPath(string? tempDir) => $"{tempDir?.TrimEnd('/')}/{_authStateFilename}";

    [Before(Assembly)]
    public static void AssemblyInitialize(AssemblyHookContext _)
    {
        if (CreateAppSettings() is { } appSettings)
        {
            if (File.Exists(AuthStateFullPath(appSettings.TEMP_DIR)))
            {
                File.Delete(AuthStateFullPath(appSettings.TEMP_DIR));
            }
        }
    }

    [After(Assembly)]
    public static async Task AssemblyCleanup()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync();
        }

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }
    }

    [Before(Test)]
    public async Task TestInitialize()
    {
        if (AppSettings is not null)
        {
            await OnPreTestInitialise();
            _browser ??= await Playwright.Firefox.LaunchAsync(new()
            {
                Headless = !Debug,
                SlowMo = Debug ? 250 : null
            });

            _context ??= await _browser.NewContextAsync(new()
            {

                StorageStatePath = File.Exists(AuthStateFullPath(AppSettings.TEMP_DIR))
                    ? AuthStateFullPath(AppSettings.TEMP_DIR)
                    : null
            });
            CurrentPage = await _context.NewPageAsync();

            await CurrentPage.GotoAsync(AppSettings.WEB_ORIGIN);

            await OnTestInitialise();
        }
    }

    protected virtual Task OnPreTestInitialise() => Task.CompletedTask;
    protected virtual Task OnTestInitialise() => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        var testContext = TestContext.Current;

        if ((testContext?.Execution?.Result?.State is TestState.Failed or TestState.Timeout) &&
            !string.IsNullOrEmpty(AppSettings.TEMP_DIR))
        {
            await CurrentPage.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = AppSettings.TEMP_DIR + "screenshot_" + testContext.Metadata.TestName + Guid.NewGuid().ToString().Replace("-", "_") + ".png",
                Type = ScreenshotType.Png
            });
        }
        else if (testContext?.Execution?.Result?.State is TestState.Passed &&
            !string.IsNullOrEmpty(AppSettings.TEMP_DIR))
        {
            if (_context != null)
            {
                await _context.StorageStateAsync(new()
                {
                    Path = AuthStateFullPath(AppSettings.TEMP_DIR)
                });
            }
        }

        await CurrentPage.CloseAsync();

        GC.SuppressFinalize(this);
    }

    protected PlaywrightAppSettings AppSettings { get; } = default!;
    protected AuthenticationHelper AuthenticationHelper { get; } = default!;
    protected IPage CurrentPage { get; set; } = default!;

    protected virtual bool Debug => Debugger.IsAttached;

    protected string GetSentence(int words = 3) => _faker.Lorem.Sentence(words);
    protected string GetNoun() => _faker.Hacker.Noun();
}
