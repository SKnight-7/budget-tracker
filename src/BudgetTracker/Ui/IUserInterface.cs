namespace BudgetTracker.Ui;

public interface IUserInterface
{
    void DisplayMessage(string message);
    void DisplayBudgets(string formattedBudgets);
    void DisplayTransactions(string formattedTransactions);
    void DisplayMainMenu(string mainMenu);
    void DisplayBudgetMenu(string budgetMenu);
    void DisplayError(string errorMessage);
    void DisplayWhimsy(bool enableWhimsy);

    string GetUserInput(string inputPrompt);
    string GetOptionNumber(string optionNumberPrompt);
    string GetFilename(string filenamePrompt);
}