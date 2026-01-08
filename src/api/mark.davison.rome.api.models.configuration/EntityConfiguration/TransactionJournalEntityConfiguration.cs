namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class TransactionJournalEntityConfiguration : RomeEntityConfiguration<TransactionJournal>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionJournal> builder)
    {
        builder
            .Property(_ => _.Description)
            .HasMaxLength(1024);

        builder
            .Property(_ => _.Date);

        builder
            .Property(_ => _.Order);

        builder
            .Property(_ => _.Completed);

        builder
            .HasMany(_ => _.Transactions)
            .WithOne(_ => _.TransactionJournal)
            .HasForeignKey(_ => _.TransactionJournalId);
    }
}
