namespace mark.davison.rome.api.models.Entities;

public class TransactionJournalTag
{
    public Guid TransactionJournalId { get; set; }
    public Guid TagId { get; set; }

    public virtual TransactionJournal? TransactionJournal { get; set; }
    public virtual Tag? Tag { get; set; }
}
