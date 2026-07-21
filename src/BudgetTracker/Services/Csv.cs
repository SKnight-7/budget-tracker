namespace BudgetTracker.Services;

/// <summary>
/// The CSV craft in one place: how to make field text that survives commas
/// and quote marks. Shared by every manager that writes or reads CSV files.
/// </summary>
public static class Csv
{
    /// <summary>Returns the value wrapped in quotes, with any quote marks inside
    /// it doubled — the CSV rules that keep commas-in-values from splitting a row.</summary>
    public static string Field(string toWrap) =>
        $"\"{toWrap.Replace("\"", "\"\"")}\"";
}