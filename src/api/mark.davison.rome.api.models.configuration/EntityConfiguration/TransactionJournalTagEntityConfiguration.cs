namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public sealed class TransactionJournalTagEntityConfiguration : IEntityTypeConfiguration<TransactionJournalTag>
{
    public void Configure(EntityTypeBuilder<TransactionJournalTag> builder)
    {
        builder
            .HasKey(_ => new { _.TransactionJournalId, _.TagId });

        builder
            .HasOne(_ => _.TransactionJournal)
            .WithMany(_ => _.JournalTags)
            .HasForeignKey(_ => _.TransactionJournalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(_ => _.Tag)
            .WithMany(_ => _.JournalTags)
            .HasForeignKey(_ => _.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
