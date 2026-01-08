namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class AccountTypeEntityConfiguration : RomeEntityConfiguration<AccountType>
{
    public override void ConfigureEntity(EntityTypeBuilder<AccountType> builder)
    {
        builder
            .Property(_ => _.Type)
            .HasMaxLength(127);
    }
}
