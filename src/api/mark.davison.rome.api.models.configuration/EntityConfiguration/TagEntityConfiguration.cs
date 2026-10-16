namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class TagEntityConfiguration : RomeEntityConfiguration<Tag>
{
    public override void ConfigureEntity(EntityTypeBuilder<Tag> builder)
    {
        builder
            .Property(_ => _.Name)
            .HasMaxLength(NameMaxLength);

        builder
            .HasIndex(_ => new { _.UserId, _.Name })
            .IsUnique();
    }
}
