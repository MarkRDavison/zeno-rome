[assembly: InternalsVisibleTo("mark.davison.rome.web.test")]

namespace mark.davison.rome.web.components;

public sealed class Routes
{
    public const string Dashboard = "/";
    public const string UserSettings = "/settings/user";
    public const string AdminSettings = "/settings/admin";
    public const string Login = "/login";
    public const string NotFound = "/not-found";
}

public sealed class RouteHelpers
{

}