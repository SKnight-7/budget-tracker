using BudgetTracker.Ui;

namespace BudgetTracker.Ui.Cli;

/// <summary>
/// The console implementation of <see cref="IInput"/>: writes a prompt to
/// the terminal and waits for a typed line. Its sibling
/// <see cref="ConsoleDisplay"/> implements <see cref="IDisplay"/>; the two
/// are separate classes because the two contracts are separate and the
/// halves share nothing.
/// </summary>
public class ConsoleInput : IInput
{
    /// <inheritdoc/>
    /// <remarks>Treats end-of-input (Ctrl+Z on Windows) as a quit request by returning "q".</remarks>
    public string GetUserInput(string inputPrompt)
    {
        Console.Write(inputPrompt);
        return Console.ReadLine() ?? "q";
    }

    /// <inheritdoc/>
    /// <remarks>Treats end-of-input (Ctrl+Z on Windows) as a quit request by returning "q".</remarks>
    public string GetOptionNumber(string optionNumberPrompt)
    {
        Console.Write(optionNumberPrompt);
        return Console.ReadLine() ?? "q";
    }

    /// <inheritdoc/>
    /// <remarks>Treats end-of-input (Ctrl+Z on Windows) as a quit request by returning "q".</remarks>
    public string GetFileName(string fileNamePrompt)
    {
        Console.Write(fileNamePrompt);
        return Console.ReadLine() ?? "q";
    }
}
