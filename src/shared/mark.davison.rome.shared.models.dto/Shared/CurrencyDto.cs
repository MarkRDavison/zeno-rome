namespace mark.davison.rome.shared.models.dto.Shared;

public record CurrencyDto(Guid Id, string Code, string Name, string Symbol, int DecimalPlaces);