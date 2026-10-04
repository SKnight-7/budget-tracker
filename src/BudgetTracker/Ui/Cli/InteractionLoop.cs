using BudgetTracker.Controllers;
using BudgetTracker.Models;
using BudgetTracker.Infrastructure;

namespace BudgetTracker.Ui.Cli;

public class InteractionLoop
{
    private readonly FinancialController _controller;
    private readonly IDisplay _display;
    private readonly IInput _input;

    public InteractionLoop(FinancialController controller, IDisplay display, IInput input)
    {
        _controller = controller;
        _display = display;
        _input = input;
    }

    public void Run()
    {
        try
        {
            _controller.LoadSavedData();
        }
        catch
        {
            _display.DisplayError("placeholder");
        }

        _display.DisplayWhimsy(true);
        while (true)
        {
            _display.DisplayMainMenu(_controller.AppActions, "MAIN MENU");

            string selection = _input.GetString("Please enter an option number, or 'q' to exit: ");

            if (selection.Equals("q", StringComparison.OrdinalIgnoreCase))
                return;

            switch (selection)
            {
                case "1":
                    _display.DisplayBudgets(_controller.GetSnapshot());
                    continue;

                case "2":
                    UpdateBudgets();
                    break;

                case "3":
                    ImportTransactions();
                    break;

                case "4":
                    ViewTransactionsByCategory();
                    break;

                case "5":
                    ViewTransactionsInOriginalOrder();
                    break;

                case "6":

                    break;

                default:
                    _display.DisplayError("Please enter a number from the menu, or 'q' to exit.");
                    continue;
            }
        }
    }

    // Updates budget amounts until the user quits: shows current budgets,
    // then repeatedly offers the category menu; a chosen category starts an
    // inner loop prompting for the new amount, and a valid amount is saved
    // through the controller and the updated budgets are shown. 'q' backs
    // out one level at a time: amount prompt to category menu, category
    // menu to main menu. Invalid entries at either prompt reprompt.
    private void UpdateBudgets()
    {
        _display.DisplayBudgets(_controller.GetSnapshot());
        while (true)
        {
            _display.DisplayBudgetMenu(_controller.GetBudgetOptions(), "AVAILABLE CATEGORIES");
            int? selectedOptionNumber = _input.GetIntIdentifier(
                "Please select a budget to update.\nEnter the option number or 'q' to exit: ");

            if (selectedOptionNumber == IInput.QuitSignal)
                break;

            BudgetCategory? selectedCategory = _controller.FindCategory(selectedOptionNumber);

            if (selectedCategory is null)
            {
                _display.DisplayError("Please select from the available option numbers.");
                continue;
            }

            while (true)
            {
                decimal? selectedAmount = _input.GetBudgetAmount(
                    $"Please enter a budget amount for {selectedCategory.Name} in the format #####.##\n"
                    + "or 'q' to choose a different category:\n");

                if (selectedAmount == IInput.QuitSignal)
                    break;

                if (selectedAmount is null)
                {
                    _display.DisplayError("Invalid entry, please try again.");
                    continue;
                }

                _controller.UpdateBudgetAmount(selectedCategory, selectedAmount.Value);
                _display.DisplayBudgets(_controller.GetSnapshot());
                break;
            }
        }
    }

    // Imports a bank file: tells the user where files must live, prompts
    // for a filename (appending .csv when the extension is missing), and
    // reprompts after a missing file or one that does not match the
    // bank's layout. A successful import displays the updated budgets
    // and returns to the main menu; 'q' returns without importing.
    private void ImportTransactions()
    {
        _display.DisplayMessage(
            $"Please enter the name of a CSV file located here: {FolderPaths.BankTransactions}\n"
            + "or enter 'q' to return to the main menu.");
        while (true)
        {
            string potentialFileName = _input.GetString("Filename: ");
            if (potentialFileName.Equals("q", StringComparison.OrdinalIgnoreCase))
                return;

            if (!potentialFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                potentialFileName = potentialFileName + ".csv";

            try
            {
                if (!_controller.ImportBankFile(potentialFileName))
                {
                    _display.DisplayError(
                        $"No file named '{potentialFileName}' was found in the BankTransactions folder.\n"
                        + "Please try again, or enter 'q' to return to the main menu.");
                    continue;
                }
            }
            catch (InvalidDataException exception)
            {
                _display.DisplayError(exception.Message);
                _display.DisplayError("Please try again, or enter 'q' to return to the main menu.");
                continue;
            }
            _display.DisplayBudgets(_controller.GetSnapshot());
            return;
        }
    }

    // Shows the current transactions ordered by category, case-insensitively;
    // the display receives a sorted copy and the live batch is never reordered.
    private void ViewTransactionsByCategory()
    {
        TransactionBatch batch = _controller.GetLastUpload();
        List<Transaction> sortedTransactions = [.. batch.Transactions
                        .OrderBy(transaction => transaction.Category, StringComparer.OrdinalIgnoreCase)];
        _display.DisplayTransactions(new(sortedTransactions, batch.SourceFileName));
    }

    // Shows the current transactions in bank-file order, rebuilt from the
    // transaction numbers rather than trusted from the list's position;
    // the display receives a sorted copy and the live batch is never reordered.
    private void ViewTransactionsInOriginalOrder()
    {
        TransactionBatch batch = _controller.GetLastUpload();
        List<Transaction> orderedTransactions = [.. batch.Transactions
                        .OrderBy(transaction => transaction.Number)];
        _display.DisplayTransactions(new(orderedTransactions, batch.SourceFileName));
    }
}
