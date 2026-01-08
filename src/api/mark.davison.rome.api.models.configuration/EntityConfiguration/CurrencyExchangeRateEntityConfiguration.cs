namespace mark.davison.rome.api.models.configuration.EntityConfiguration;

public partial class CurrencyExchangeRateEntityConfiguration : RomeEntityConfiguration<CurrencyExchangeRate>
{
    public override void ConfigureEntity(EntityTypeBuilder<CurrencyExchangeRate> builder)
    {
        builder
            .Property(_ => _.Date);

        builder
            .Property(_ => _.Rate);

        builder
            .Property(_ => _.UserRate);
    }
}
