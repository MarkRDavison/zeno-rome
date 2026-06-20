namespace mark.davison.rome.web.components.Pages.Transaction.View;

internal class ViewTransactionItem
{
    public required string Description { get; init; }
    public required LinkDefinition SourceAccount { get; init; }
    public required LinkDefinition DestinationAccount { get; init; }
    public required string Amount { get; init; }
    public required long AmountValue { get; init; }
    public required string? ForeignAmount { get; init; }
    public required LinkDefinition? Category { get; init; }
    public required string AmountStyle { get; init; }
}