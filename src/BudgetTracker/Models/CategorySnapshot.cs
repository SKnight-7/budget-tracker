namespace BudgetTracker.Models;

public class CategorySnapshot
{
    /// <summary>The GeneralClassification carried by the one appended row
    /// that holds all unbudgeted money. Code tests against this value to
    /// give that row its own treatment: FinancialSnapshot keeps it out of
    /// the overview arithmetic, and the views display it beside the tables
    /// rather than inside them. You can never budget for "Unbudgeted", so
    /// the value belongs to snapshots, not to BudgetCategory.</summary>
    public const string UnbudgetedClassification = "Unbudgeted";

    public string GeneralClassification { get; }
    public string CategoryName { get; }
    public decimal BudgetedAmount { get; }
    public decimal ActualAmount { get; }
    public decimal Difference { get; }

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

