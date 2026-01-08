namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class TransactionTypeEntityConfiguration : RomeEntityConfiguration<TransactionType>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionType> builder)
    {
        builder
            .Property(_ => _.Type)
            .HasMaxLength(50);
    }
}
