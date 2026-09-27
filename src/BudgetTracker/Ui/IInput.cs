namespace BudgetTracker.Ui;

/// <summary>
/// The contract for collecting user input. Implementable by any front end
/// that can ask a question and wait for the answer: the console, a dialog
/// box, or a scripted test fake that answers from a prepared list. Front
/// ends that cannot wait (a web server, where the user's browser initiates
/// every exchange) cannot implement this contract and interact through
/// their own layer instead.
/// </summary>
public interface IInput
{
    /// <summary>Prompts for and returns a line of user input. Never returns null.</summary>
    string GetUserInput(string inputPrompt);

    /// <summary>Prompts for a menu option number. Never returns null.</summary>
    string GetOptionNumber(string optionNumberPrompt);

    /// <summary>Prompts for a filename. Never returns null.</summary>
    string GetFileName(string fileNamePrompt);
}
