using System.Globalization;

namespace BudgetTracker.Parsing;

/// <summary>
/// Wraps the standard TryParse patterns in methods that return null on failure instead
/// of throwing exceptions, so callers can validate raw user input with a simple null check.
/// </summary>
public static class Parser
{
    /// <summary>Parses text as an integer.</summary>
    /// <param name="potentialInt">The text to parse, typically raw user input.</param>
    /// <returns>The parsed value, or null if the text is not a valid integer.</returns>
    public static int? ParseInt(string potentialInt) =>
        int.TryParse(potentialInt, out int parsedInt) ? parsedInt : null;

    /// <summary>Parses text as a decimal.</summary>
    /// <param name="potentialDecimal">The text to parse, typically raw user input.</param>
    /// <returns>The parsed value, or null if the text is not a valid decimal.</returns>
    public static decimal? ParseDecimal(string potentialDecimal) =>
        decimal.TryParse(potentialDecimal, out decimal parsedDecimal) ? parsedDecimal : null;

    /// <summary>Parses text as a date in one exact format.</summary>
    /// <param name="potentialDate">The text to parse, typically raw user input.</param>
    /// <param name="expectedFormat">The exact format the text must match (for example, "MM/dd/yyyy").</param>
    /// <returns>The parsed date, or null if the text does not match the expected format.</returns>
    /// <remarks>
    /// The caller supplies the format, so each data source can declare its own shape.
    /// Characters with no special meaning in the format must match literally, which
    /// accommodates any separator a source uses, and "M" or "d" accept one or two
    /// digits while "MM" or "dd" require exactly two. Uses the invariant culture so
    /// stored data parses identically on any machine.
    /// </remarks>
    public static DateOnly? ParseDate(string potentialDate, string expectedFormat) =>
    DateOnly.TryParseExact(potentialDate, expectedFormat, CultureInfo.InvariantCulture,
        DateTimeStyles.None, out DateOnly parsedDate) ? parsedDate : null;
}
