namespace mark.davison.rome.shared.models.dto.Shared;

public sealed record CreateTransactionDto(
    Guid Id,
    string Description,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    DateOnly Date,
    long Amount,
    long? ForeignAmount,
    Guid CurrencyId,
    Guid? ForeignCurrencyId
);
