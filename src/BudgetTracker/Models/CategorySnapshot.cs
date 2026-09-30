namespace BudgetTracker.Models;

/// <summary>
/// One row of the budgets display: a single category's numbers, detached
/// and frozen at the moment of creation. Every value is copied or computed
/// in the constructor, so a snapshot always agrees with itself and never
/// changes when the live budget objects change afterward. Created by
/// SnapshotGenerator, read by displays, stored by nothing.
/// </summary>
public class CategorySnapshot
{
    /// <summary>The GeneralClassification carried by the one appended row
    /// that holds all unbudgeted money. Code tests against this value to
    /// give that row its own treatment: FinancialSnapshot keeps it out of
    /// the overview arithmetic, and the views display it beside the tables
    /// rather than inside them. You can never budget for "Unbudgeted", so
    /// the value belongs to snapshots, not to BudgetCategory.</summary>
    public const string UnbudgetedClassification = "Unbudgeted";

    /// <summary>The classification copied from the budget category, compared
    /// against BudgetCategory.IncomeClassification to decide which table the
    /// row belongs to. The appended unbudgeted row carries
    /// UnbudgetedClassification instead.</summary>
    public string GeneralClassification { get; }

    /// <summary>The category's name, copied at creation.</summary>
    public string CategoryName { get; }

    /// <summary>The amount budgeted for the category at the moment the
    /// snapshot was made. Zero on the appended unbudgeted row.</summary>
    public decimal BudgetedAmount { get; }

    /// <summary>What actually happened in the category: money received for
    /// income, money spent for expenses, positive when the money moved the
    /// expected way. The sign flip from bank convention happens before the
    /// snapshot is created.</summary>
    public decimal ActualAmount { get; }

    /// <summary>BudgetedAmount minus ActualAmount, computed in the
    /// constructor so the equation cannot be false for any snapshot that
    /// exists: what is still expected to arrive for income, or what is
    /// still available to spend for an expense.</summary>
    public decimal Difference { get; }

    /// <summary>
    /// Copies the given values and computes Difference from the parameters.
    /// The constructor does not check its arguments: every present caller
    /// passes values that the models' own constructors and setters already
    /// validated.
    /// </summary>
    public CategorySnapshot(
        string generalClassification,
        string categoryName,
        decimal budgetedAmount,
        decimal actualAmount)
    {
        GeneralClassification = generalClassification;
        CategoryName = categoryName;
        BudgetedAmount = budgetedAmount;
        ActualAmount = actualAmount;
        Difference = budgetedAmount - actualAmount;
    }
}
