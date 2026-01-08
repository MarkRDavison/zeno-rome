namespace mark.davison.rome.shared.accounting.rules;

public static class DateRules
{
    public static (DateOnly, DateOnly) GetMonthRange(this DateOnly dateOnly)
    {
        var endYear = dateOnly.Month == 12 ? dateOnly.Year + 1 : dateOnly.Year;
        var endMonth = dateOnly.Month == 12 ? 1 : dateOnly.Month + 1;

        var start = new DateOnly(dateOnly.Year, dateOnly.Month, 1);
        var end = new DateOnly(endYear, endMonth, 1).AddDays(-1);

        return (start, end);
    }
}