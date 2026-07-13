namespace BudgetTracker.Ui;

/// <summary>
/// The contract for all user interaction. Application logic talks to this interface
/// rather than to the console directly, so a different front end could be swapped in
/// without touching the rest of the program.
/// </summary>
public interface IUserInterface
{
    /// <summary>Displays a general-purpose message.</summary>
    void DisplayMessage(string message);

    /// <summary>Displays the formatted budgets-with-expenditures view.</summary>
    void DisplayBudgets(string formattedBudgets);

    /// <summary>Displays the formatted transactions listing.</summary>
    void DisplayTransactions(string formattedTransactions);

    /// <summary>Displays the main menu.</summary>
    void DisplayMainMenu(string mainMenu);

    /// <summary>Displays the budget category menu.</summary>
    void DisplayBudgetMenu(string budgetMenu);

    /// <summary>Displays an error message. Kept separate from DisplayMessage so an
    /// implementation can style errors differently.</summary>
    void DisplayError(string errorMessage);

    /// <summary>Displays the app greeting, or a plain welcome when whimsy is disabled.</summary>
    void DisplayWhimsy(bool enableWhimsy);

    /// <summary>Prompts for and returns a line of user input. Never returns null.</summary>
    string GetUserInput(string inputPrompt);

    /// <summary>Prompts for a menu option number. Never returns null.</summary>
    string GetOptionNumber(string optionNumberPrompt);

    /// <summary>Prompts for a filename. Never returns null.</summary>
    string GetFilename(string filenamePrompt);
}
