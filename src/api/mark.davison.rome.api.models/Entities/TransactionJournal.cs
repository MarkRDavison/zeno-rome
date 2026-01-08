namespace mark.davison.rome.api.models.Entities;

public class TransactionJournal : RomeEntity
{
    public Guid TransactionTypeId { get; set; }
    public Guid TransactionGroupId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid? ForeignCurrencyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int Order { get; set; }
    public bool Completed { get; set; }

    public virtual TransactionType? TransactionType { get; set; }
    public virtual TransactionGroup? TransactionGroup { get; set; }
    public virtual List<Transaction> Transactions { get; set; } = [];
    public virtual Currency? Currency { get; set; }
    public virtual Currency? ForeignCurrency { get; set; }
}