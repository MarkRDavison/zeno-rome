namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public sealed class UserPreferenceEntityConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(_ => _.UserId);

        builder
            .Property(_ => _.PreferencesJson)
            .IsRequired();

        builder
            .HasOne(_ => _.User)
            .WithMany()
            .HasForeignKey(_ => _.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
