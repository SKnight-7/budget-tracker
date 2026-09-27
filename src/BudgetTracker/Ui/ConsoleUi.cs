using BudgetTracker.Models;

namespace BudgetTracker.Ui;

/// <summary>
/// The console implementation of <see cref="IDisplay"/> and <see cref="IInput"/>:
/// writes to and reads from the terminal.
/// </summary>
public class ConsoleUi : IDisplay, IInput
{
    /// <inheritdoc/>
    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc/>
    public void DisplayError(string errorMessage)
    {
        Console.WriteLine(errorMessage);
    }

    /// <inheritdoc/>
    public void DisplayWhimsy(bool enableWhimsy)
    {
        Console.Write(Whimsy.ApplyWhimsy(enableWhimsy));
    }

    /// <inheritdoc/>
    public void DisplayMainMenu(List<MenuOption> options, string menuTitle)
    {
        Console.WriteLine(Menus.Generate(options, menuTitle));
    }

    /// <inheritdoc/>
    /// <remarks>Passes no column count, so Menus.GenerateColumned's default
    /// (four columns) applies; to change the printed column count, pass an
    /// explicit third argument here. IDisplay.DisplayBudgetMenu deliberately
    /// has no column parameter because no front end other than CLI
    /// arranges options into character columns.</remarks>
    public void DisplayBudgetMenu(List<MenuOption> options, string menuTitle)
    {
        Console.WriteLine(Menus.GenerateColumned(options, menuTitle));
    }

    /// <inheritdoc/>
    public void DisplayBudgets(FinancialSnapshot snapshot)
    {
        // To be built.
        // Originally, was "Console.WriteLine(formattedBudgets);"
        // That assumed receiving a string and displaying it, but
        // this will need to build that string from the snapshot.
    }

    /// <inheritdoc/>
    public void DisplayTransactions(TransactionBatch batch)
    {
        string displayTitle = "LAST UPLOADED TRANSACTIONS";
        List<string> headers = ["Transaction #", "Date", "Amount", "Description", "Category"];
        List<Alignment> alignments =
            [Alignment.Right, Alignment.Center, Alignment.Right, Alignment.Left, Alignment.Left];

        List<List<string>> rows = [.. batch.Transactions
            .Select(transaction => new List<string>
            {
                transaction.Number.ToString(),
                transaction.Date.ToString("yyyy-MM-dd"),
                transaction.Amount.ToString("C"),
                transaction.Description,
                transaction.Category,
            })];

        Console.WriteLine($"{TextLayout.Underline(displayTitle)}\n"
                        + $"Transactions from: {batch.SourceFileName}\n"
                        + "\n"
                        + Tables.Generate(headers, rows, alignments));
    }

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
