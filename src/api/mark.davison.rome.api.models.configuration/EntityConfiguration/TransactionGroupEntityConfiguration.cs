namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class TransactionGroupEntityConfiguration : RomeEntityConfiguration<TransactionGroup>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionGroup> builder)
    {
        builder
            .Property(_ => _.Title)
            .HasMaxLength(NameMaxLength);
    }
}
