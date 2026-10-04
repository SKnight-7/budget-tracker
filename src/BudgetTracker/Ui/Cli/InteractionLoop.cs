using BudgetTracker.Controllers;
using BudgetTracker.Models;

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

                    break;

                case "4":

                    break;

                case "5":

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
}
