namespace mark.davison.rome.api.models.Entities;

public class Tag : RomeEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual List<TransactionJournalTag> JournalTags { get; set; } = [];
}
