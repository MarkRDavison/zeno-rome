namespace mark.davison.common.web.playwright.test.Core;

public class PlaywrightAppSettings
{
    public string Section => "COMMON_PLAYWRIGHT";
    public bool Validate()
    {
        return
            !string.IsNullOrEmpty(WEB_ORIGIN) &&
            !string.IsNullOrEmpty(APP_TITLE) &&
            !string.IsNullOrEmpty(AUTH_PROVIDER) &&
            !string.IsNullOrEmpty(AUTH_PASSWORD) &&
            !string.IsNullOrEmpty(APP_TITLE);
    }
    public string TEMP_DIR { get; set; } = string.Empty;
    public string WEB_ORIGIN { get; set; } = string.Empty;
    public string AUTH_PROVIDER { get; set; } = string.Empty;
    public string AUTH_USERNAME { get; set; } = string.Empty;
    public string AUTH_PASSWORD { get; set; } = string.Empty;
    public string APP_TITLE { get; set; } = string.Empty;
}
