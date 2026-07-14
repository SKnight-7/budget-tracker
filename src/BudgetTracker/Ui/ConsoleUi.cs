namespace BudgetTracker.Ui;

/// <summary>
/// The console implementation of <see cref="IUserInterface"/>: 
/// writes to and reads from the terminal.
/// </summary>
public class ConsoleUi : IUserInterface
{
    /// <inheritdoc/>
    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc/>
    public void DisplayBudgets(string formattedBudgets)
    {
        Console.WriteLine(formattedBudgets);
    }
    
    /// <inheritdoc/>
    public void DisplayTransactions(string formattedTransactions)
    {
        Console.WriteLine(formattedTransactions);
    }
    
    /// <inheritdoc/>
    public void DisplayMainMenu(string mainMenu)
    {
        Console.WriteLine(mainMenu);
    }
    
    /// <inheritdoc/>
    public void DisplayBudgetMenu(string budgetMenu)
    {
        Console.WriteLine(budgetMenu);
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
    public string GetFilename(string filenamePrompt)
    {
        Console.Write(filenamePrompt);
        return Console.ReadLine() ?? "q";
    }
}