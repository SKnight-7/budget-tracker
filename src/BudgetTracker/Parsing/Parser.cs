using System.Globalization;

namespace BudgetTracker.Parsing;

public static class Parser
{
    public static int? ParseInt(string potentialInt) =>
        int.TryParse(potentialInt, out int parsedInt) ? parsedInt : null;

    public static decimal? ParseDecimal(string potentialDecimal) =>
        decimal.TryParse(potentialDecimal, out decimal parsedDecimal) ? parsedDecimal : null;

    public static DateOnly? ParseDate(string potentialDate, string expectedFormat) =>
    DateOnly.TryParseExact(potentialDate, expectedFormat, CultureInfo.InvariantCulture,
        DateTimeStyles.None, out DateOnly parsedDate) ? parsedDate : null;
}