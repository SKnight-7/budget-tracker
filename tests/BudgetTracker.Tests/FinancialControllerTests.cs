using BudgetTracker.Controllers;
using BudgetTracker.Managers;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for the FinancialController. The catalog test is the suite-side
/// counterpart of the startup tripwire in InteractionLoop.Run: the tripwire
/// stops a drifted program from running, and the test here stops the drift
/// from surviving a build, so a catalog change that forgets the switch is
/// caught before anyone even launches the app.
/// </summary>
public class FinancialControllerTests
{
    // Builds a controller on repositories pointed at test file names, the
    // same pattern the repository tests use, so the app's real data files
    // are never touched. The catalog lives on the controller itself, so
    // these repositories are never read; they exist because the managers
    // require them at construction.
    private static FinancialController BuildController() =>
        new(
            new BudgetManager(new JsonBudgetRepository("financialControllerTests_budgets.json")),
            new TransactionManager(
                new WellsFargoTransactionSource(),
                new JsonTransactionRepository("financialControllerTests_transactions.json")));

    /// <summary>The controller offers exactly the six actions the
    /// interaction loop dispatches, in option-number order.</summary>
    [Fact]
    public void AppActions_AvailableActions_MatchExpected()
    {
        // The actions the interaction loop was built to handle: the switch
        // in InteractionLoop.Run dispatches exactly these, so this list,
        // that switch, the loop's own expectedActions, and the controller's
        // catalog must always change together.
        List<Option> expectedActions =
        [
            new("Budget Options", "View current budgets", 1),
            new("Budget Options", "Update budgets", 2),
            new("Transaction Options", "Choose a CSV transaction file to load", 3),
            new("Transaction Options", "View transactions by category", 4),
            new("Transaction Options", "View transactions in original order", 5),
            new("Transaction Options", "Recategorize transactions", 6),
        ];

        Assert.Equal(expectedActions, BuildController().AppActions);
    }
}
