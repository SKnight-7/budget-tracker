namespace BudgetTracker.Ui;

public class ConsoleUi : IUserInterface
{
    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void DisplayBudgets(string formattedBudgets)
    {
        Console.WriteLine(formattedBudgets);
    }
    
    public void DisplayTransactions(string formattedTransactions)
    {
        Console.WriteLine(formattedTransactions);
    }
    
    public void DisplayMainMenu(string mainMenu)
    {
        Console.WriteLine(mainMenu);
    }
    
    public void DisplayBudgetMenu(string budgetMenu)
    {
        Console.WriteLine(budgetMenu);
    }
    
    public void DisplayError(string errorMessage)
    {
        Console.WriteLine(errorMessage);
    }
    
    public void DisplayWhimsy(bool enableWhimsy)
    {
        Console.Write(Whimsy.ApplyWhimsy(enableWhimsy));
    }

    public string GetUserInput(string inputPrompt)
    {
        Console.Write(inputPrompt);
        return Console.ReadLine() ?? "q";
    }
        
    public string GetOptionNumber(string optionNumberPrompt)
    {
        Console.Write(optionNumberPrompt);
        return Console.ReadLine() ?? "q";
    }

    public string GetFilename(string filenamePrompt)
    {
        Console.Write(filenamePrompt);
        return Console.ReadLine() ?? "q";
    }
}