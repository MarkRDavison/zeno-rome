namespace mark.davison.rome.shared.models.dto.Shared;

public record StartupDataDto(
    UserContextDto UserContext,
    List<AccountTypeDto> AccountTypes,
    List<CurrencyDto> Currencies,
    List<TransactionTypeDto> TransactionTypes);
