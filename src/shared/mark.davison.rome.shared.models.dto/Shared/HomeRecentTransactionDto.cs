namespace mark.davison.rome.shared.models.dto.Shared;

public sealed record HomeRecentTransactionDto(
    Guid TransactionGroupId,
    DateOnly Date,
    string Title,
    Guid? TransactionTypeId
);
