namespace mark.davison.rome.shared.models.dto.Shared;

public sealed class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid TransactionJournalId { get; set; }
    public Guid TransactionGroupId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid? ForeignCurrencyId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? SplitTransactionDescription { get; set; }
    public DateOnly Date { get; set; }
    public long Amount { get; set; }
    public long? ForeignAmount { get; set; }
    public bool Reconciled { get; set; }
    public bool Source { get; set; }
    public Guid TransactionTypeId { get; set; }
}