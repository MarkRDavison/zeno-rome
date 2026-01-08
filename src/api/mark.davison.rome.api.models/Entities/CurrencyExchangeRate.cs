namespace mark.davison.rome.api.models.Entities;

public class CurrencyExchangeRate : RomeEntity
{
    public Guid FromCurrencyId { get; set; }
    public Guid ToCurrencyId { get; set; }
    public DateOnly Date { get; set; }
    public long Rate { get; set; }
    public long UserRate { get; set; }

    public virtual Currency? FromCurrency { get; set; }
    public virtual Currency? ToCurrency { get; set; }
}
