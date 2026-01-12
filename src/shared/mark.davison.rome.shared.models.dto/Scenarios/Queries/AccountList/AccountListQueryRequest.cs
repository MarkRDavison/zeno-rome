namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.AccountList;

[GetRequest(Path = "account-list-query")]
public sealed class AccountListQueryRequest : IQuery<AccountListQueryRequest, AccountListQueryResponse>
{
    public Guid? AccountType { get; set; }
}
