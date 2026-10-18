namespace mark.davison.rome.shared.models.dto.Shared;

public sealed record UpsertAccountDto(
    Guid Id,
    string Name,
    long? VirtualBalance,
    string AccountNumber,
    Guid AccountTypeId,
    Guid CurrencyId,
    long? OpeningBalance,
    DateOnly? OpeningBalanceDate,
    bool IsFavourite = false
)
{
    public static UpsertAccountDto Default => new(Guid.Empty, string.Empty, null, string.Empty, Guid.Empty, Guid.Empty, null, null);
}