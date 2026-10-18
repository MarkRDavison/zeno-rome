namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.AccountById;

[GetRequest(Path = "account-by-id-query")]
public sealed class AccountByIdQueryRequest : IQuery<AccountByIdQueryRequest, AccountByIdQueryResponse>
{
    public Guid AccountId { get; set; }
}
