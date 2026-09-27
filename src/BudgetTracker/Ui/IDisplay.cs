using BudgetTracker.Models;

namespace BudgetTracker.Ui;

/// <summary>
/// The interface for displaying to the user. Every method receives data,
/// never pre-formatted output, so each implementing class does all of its
/// own formatting: a console implementation builds text, a web
/// implementation would build markup. Classes that need something shown
/// hold a reference typed as IDisplay and call these methods; only
/// Program.cs names the implementing class, so swapping the display means
/// changing that one line. Input methods are in a separate interface
/// (<see cref="IInput"/>) because some front ends, such as a web server,
/// can implement displays but not ask-and-wait input.
/// </summary>
public interface IDisplay
{
    /// <summary>Displays a general-purpose message.</summary>
    void DisplayMessage(string message);

    /// <summary>Displays an error message. Kept separate from DisplayMessage so an
    /// implementation can style errors differently.</summary>
    void DisplayError(string errorMessage);

    /// <summary>Displays the app greeting, or a plain welcome when whimsy is disabled.</summary>
    void DisplayWhimsy(bool enableWhimsy);

    /// <summary>Displays what the app can do: each action's label next to
    /// the option number the user enters to start it, grouped under the
    /// given title.</summary>
    void DisplayMainMenu(List<MenuOption> options, string menuTitle);

    /// <summary>Displays each budget category's name next to the option
    /// number the user enters to select it, for flows that need a category
    /// chosen: setting a budgeted amount, or recategorizing a transaction.
    /// How the categories are arranged on screen is each implementing
    /// class's own decision.</summary>
    void DisplayBudgetMenu(List<MenuOption> options, string menuTitle);

    /// <summary>Displays the full financial picture in the snapshot: each
    /// income and expense category with its budgeted amount, actual amount,
    /// and difference; the six overview figures; the unbudgeted total; and
    /// the name of the bank file the numbers are based on.</summary>
    void DisplayBudgets(FinancialSnapshot snapshot);

    /// <summary>Displays the given transactions in the order they arrive.
    /// Callers wanting a particular order hand in a sorted copy.</summary>
    void DisplayTransactions(TransactionBatch batch);
}
