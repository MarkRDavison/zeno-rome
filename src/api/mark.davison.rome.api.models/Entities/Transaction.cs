namespace mark.davison.rome.api.models.Entities;

public class Transaction : RomeEntity
{
    public Guid AccountId { get; set; }
    public Guid TransactionJournalId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid? ForeignCurrencyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long? ForeignAmount { get; set; }
    public bool Reconciled { get; set; }

    public bool IsSource { get; set; }

    public virtual Account? Account { get; set; }
    public virtual TransactionJournal? TransactionJournal { get; set; }
    public virtual Currency? Currency { get; set; }
    public virtual Currency? ForeignCurrency { get; set; }
}