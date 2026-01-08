[assembly: InternalsVisibleTo("mark.davison.rome.web.test")]

namespace mark.davison.rome.web.components;

public sealed class Routes
{
    public const string Dashboard = "/";
    public const string UserSettings = "/settings/user";
    public const string AdminSettings = "/settings/admin";
    public const string Login = "/login";
    public const string NotFound = "/not-found";


    public const string Accounts = "/accounts";
    public const string AccountsByType = "/accounts/{type:guid}";
    public const string Account = "/account/{id:guid}";
    public const string AccountEdit = "/account/{id:guid}/edit";
    public const string AccountNew = "/account/new/{type:guid}";

    public const string Transactions = "/transactions";
    public const string TransactionsByType = "/transactions/{type:guid}";
    public const string Transaction = "/transaction/{id:guid}";
    public const string TransactionNew = "/transactions/new/{type:guid}";
}

public sealed class RouteHelpers
{
    public static string Account(Guid id) => Routes.Account.Replace("{id:guid}", id.ToString());
    public static string Accounts(Guid type) => Routes.AccountsByType.Replace("{type:guid}", type.ToString());
    public static string AccountNew(Guid type) => Routes.AccountNew.Replace("{type:guid}", type.ToString());
    public static string TransactionNew(Guid type) => Routes.TransactionNew.Replace("{type:guid}", type.ToString());
    public static string Transaction(Guid id) => Routes.Transaction.Replace("{id:guid}", id.ToString());
    public static string Transactions(Guid type) => Routes.TransactionsByType.Replace("{type:guid}", type.ToString());
}