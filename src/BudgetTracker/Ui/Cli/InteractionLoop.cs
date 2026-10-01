using BudgetTracker.Controllers;

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
            // To be built
        }
    }
}
