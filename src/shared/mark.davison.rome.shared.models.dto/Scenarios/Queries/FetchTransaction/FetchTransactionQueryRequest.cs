namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.FetchTransaction;

[GetRequest(Path = "fetch-transaction-query")]
public sealed class FetchTransactionQueryRequest : IQuery<FetchTransactionQueryRequest, FetchTransactionQueryResponse>
{
    public Guid TransactionGroupId { get; set; }
}
