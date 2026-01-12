namespace mark.davison.rome.shared.models.dto.Shared;

public sealed record AccountDto(
    Guid Id,
    string Name,
    Guid AccountTypeId,
    string AccountNumber,
    long CurrentBalance,
    bool Active,
    DateTimeOffset LastModified,
    long BalanceDifference,
    Guid CurrencyId,
    long? VirtualBalance,
    long? OpeningBalance,
    DateOnly? OpeningBalanceDate
);