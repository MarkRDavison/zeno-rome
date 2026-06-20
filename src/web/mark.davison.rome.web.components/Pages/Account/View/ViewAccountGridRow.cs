namespace mark.davison.rome.web.components.Pages.Account.View;

public sealed class ViewAccountGridRow
{
    public bool IsSplit { get; set; }
    public bool IsSubTransaction { get; set; }
    public LinkDefinition Description { get; set; } = new();
    public Guid TransactionGroupId { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? Date { get; set; }
    public LinkDefinition SourceAccount { get; set; } = new();
    public LinkDefinition DestinationAccount { get; set; } = new();
    public string TransactionType { get; set; } = string.Empty;
    public LinkDefinition Category { get; set; } = new();
}