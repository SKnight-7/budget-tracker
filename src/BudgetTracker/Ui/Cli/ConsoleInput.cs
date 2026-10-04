using BudgetTracker.Services;

namespace BudgetTracker.Ui.Cli;

/// <summary>
/// The console implementation of <see cref="IInput"/>: writes each prompt
/// to the terminal and reads a typed line back. The typed Gets build on
/// GetString, so every entry arrives through the same read before the
/// validation runs. Its sibling <see cref="ConsoleDisplay"/> implements
/// <see cref="IDisplay"/>; the two are separate classes because the two
/// contracts are separate and the halves share nothing.
/// </summary>
public class ConsoleInput : IInput
{
    /// <inheritdoc/>
    /// <remarks>Treats end-of-input (Ctrl+Z on Windows) as a quit request by returning "q".</remarks>
    public string GetString(string inputPrompt)
    {
        Console.Write(inputPrompt);
        return Console.ReadLine() ?? "q";
    }

    /// <inheritdoc/>
    public int? GetIntIdentifier(string inputPrompt)
    {
        string entry = GetString(inputPrompt);

        if (entry.Equals("q", StringComparison.OrdinalIgnoreCase))
            return IInput.QuitSignal;

        int? parsedEntry = Parser.ParseInt(entry);
        return parsedEntry > 0 ? parsedEntry : null;
    }

    /// <inheritdoc/>
    public decimal? GetBudgetAmount(string inputPrompt)
    {
        string entry = GetString(inputPrompt);

        if (entry.Equals("q", StringComparison.OrdinalIgnoreCase))
            return IInput.QuitSignal;

        decimal? parsedEntry = Parser.ParseDecimal(entry);

        if (parsedEntry is null || parsedEntry < 0 || parsedEntry != Math.Round(parsedEntry.Value, 2))
            return null;

        return parsedEntry;
    }
}
