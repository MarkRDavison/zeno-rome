namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.TransactionByAccount;

[GetRequest(Path = "transaction-by-account-query")]
public class TransactionByAccountQueryRequest : IQuery<TransactionByAccountQueryRequest, TransactionByAccountQueryResponse>
{
    public Guid AccountId { get; set; }
}
