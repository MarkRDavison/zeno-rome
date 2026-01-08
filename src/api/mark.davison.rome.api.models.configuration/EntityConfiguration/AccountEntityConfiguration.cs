namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class AccountEntityConfiguration : RomeEntityConfiguration<Account>
{
    public override void ConfigureEntity(EntityTypeBuilder<Account> builder)
    {
        builder
            .Property(_ => _.Name)
            .HasMaxLength(NameMaxLength);

        builder
            .Property(_ => _.IsActive);

        builder
            .Property(_ => _.VirtualBalance);

        builder
            .Property(_ => _.AccountNumber)
            .HasMaxLength(255);

        builder
            .Property(_ => _.Order);
    }
}