namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.Home;

public sealed class HomeQueryResponse : Response<HomeQueryValue>;

public sealed class HomeQueryValue
{
    public List<AccountDto> Accounts { get; set; } = [];
    public List<HomeRecentTransactionDto> RecentTransactions { get; set; } = [];
}
