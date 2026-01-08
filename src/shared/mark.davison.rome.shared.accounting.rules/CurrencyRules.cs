namespace mark.davison.rome.shared.accounting.rules;

public static class CurrencyRules
{
    public const int FinanceDecimalPlaces = 4;

    public static long ToPersisted(decimal value)
    {
        return (long)(value * (decimal)Math.Pow(10, FinanceDecimalPlaces));
    }
    public static decimal FromPersisted(long value)
    {
        return ((decimal)value / (decimal)Math.Pow(10, FinanceDecimalPlaces));
    }

    public static string FromPersistedToFormatted(long value, string symbol, int decimalPlaces)
    {
        var display = FromPersisted(value);
        bool negative = display < 0.0M;

        return $"{(negative ? "-" : "")}{symbol}{Math.Abs(display).ToString($"N{decimalPlaces}")}";
    }
}