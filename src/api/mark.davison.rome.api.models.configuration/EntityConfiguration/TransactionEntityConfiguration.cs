namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class TransactionEntityConfiguration : RomeEntityConfiguration<Transaction>
{
    public override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .Property(_ => _.Description)
            .HasMaxLength(1024);

        builder
            .Property(_ => _.Amount);

        builder
            .Property(_ => _.ForeignAmount);

        builder
            .Property(_ => _.Reconciled);

        builder
            .Property(_ => _.IsSource);
    }
}
