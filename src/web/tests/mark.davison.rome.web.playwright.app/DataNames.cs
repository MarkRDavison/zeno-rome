namespace mark.davison.rome.web.playwright.app;

public sealed class DataNames : IDataNames
{
    public static IDataNames Instance => new DataNames();

    public string Username => DataTestIds.Username;
}
