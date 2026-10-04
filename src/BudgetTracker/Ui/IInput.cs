namespace BudgetTracker.Ui;

/// <summary>
/// The interface for collecting user input. Implementable by any front end
/// that can ask a question and wait for the answer: the console, a dialog
/// box, or a scripted test fake that answers from a prepared list. Front
/// ends that cannot wait (a web server, where the user's browser initiates
/// every exchange) cannot implement this interface and interact through
/// their own layer instead. GetString returns the typed text raw; the
/// typed Gets validate the entry at the boundary and signal one of three
/// outcomes: a valid value, QuitSignal when the user asked to back out,
/// or null when the entry failed validation.
/// </summary>
public interface IInput
{
    /// <summary>The value the typed Gets return when the user enters 'q'
    /// to back out instead of answering. Never collides with a real
    /// answer: identifiers and budget amounts are never negative, and the
    /// typed Gets return null for any negative entry a user types.</summary>
    const int QuitSignal = -1;

    /// <summary>Prompts for and returns a line of user input. Never returns null.</summary>
    string GetString(string inputPrompt);

    /// <summary>Prompts for an identifying number, such as a menu option
    /// number or a transaction number.</summary>
    /// <returns>The entered identifier, always a positive integer;
    /// QuitSignal when the user entered 'q'; or null when the entry was
    /// not a positive integer. Whether the identifier matches anything
    /// currently available is a separate check that belongs to the
    /// caller.</returns>
    int? GetIntIdentifier(string inputPrompt);

    /// <summary>Prompts for a budget amount.</summary>
    /// <returns>The entered amount, never negative and with at most two
    /// decimal places; QuitSignal when the user entered 'q'; or null when
    /// the entry failed those checks.</returns>
    decimal? GetBudgetAmount(string inputPrompt);
}
