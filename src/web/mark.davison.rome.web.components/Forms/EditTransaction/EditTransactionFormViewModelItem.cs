namespace mark.davison.rome.web.components.Forms.EditTransaction;

public class EditTransactionFormViewModelItem
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? SourceAccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public decimal? Amount { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ForeignCurrencyId { get; set; }
    public decimal? ForeignAmount { get; set; }

    public bool Valid =>
        !string.IsNullOrEmpty(Description) &&
        SourceAccountId != null &&
        SourceAccountId != Guid.Empty &&
        DestinationAccountId != null &&
        DestinationAccountId != Guid.Empty &&
        Amount != null &&
        Amount > 0.0M &&
        CategoryId != Guid.Empty &&
        (ForeignCurrencyId == null ||
            (ForeignCurrencyId != Guid.Empty &&
            ForeignAmount != null &&
            ForeignAmount > 0.0M));
}
