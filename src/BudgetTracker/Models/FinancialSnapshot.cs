namespace BudgetTracker.Models;

public class FinancialSnapshot
{
    public List<CategorySnapshot> IncomeRows { get; }
    public List<CategorySnapshot> ExpenseRows { get; }
    public decimal TotalExpectedIncome { get; }
    public decimal TotalReceived { get; }
    public decimal TotalBudgetedExpenses { get; }
    public decimal TotalExpended { get; }
    public decimal AvailableToAllocate { get; }
    public decimal UnspentBalance { get; }
    public decimal UnbudgetedExpenses { get; }
    public string SourceFileName { get; }

    public FinancialSnapshot(List<CategorySnapshot> categorySnapshots, string sourceFileName)
    {
        IncomeRows = [.. categorySnapshots
        .Where(snapshot => string.Equals(
            snapshot.GeneralClassification,
            BudgetCategory.IncomeClassification,
            StringComparison.OrdinalIgnoreCase))];

        ExpenseRows = [.. categorySnapshots
            .Where(snapshot =>
                !string.Equals(
                    snapshot.GeneralClassification,
                    BudgetCategory.IncomeClassification,
                    StringComparison.OrdinalIgnoreCase)
                &&
                !string.Equals(
                    snapshot.GeneralClassification,
                    CategorySnapshot.UnbudgetedClassification,
                    StringComparison.OrdinalIgnoreCase))];

        TotalExpectedIncome = IncomeRows.Sum(snapshot => snapshot.BudgetedAmount);
        TotalReceived = IncomeRows.Sum(snapshot => snapshot.ActualAmount);
        TotalBudgetedExpenses = ExpenseRows.Sum(snapshot => snapshot.BudgetedAmount);
        TotalExpended = ExpenseRows.Sum(snapshot => snapshot.ActualAmount);

        AvailableToAllocate = TotalExpectedIncome - TotalBudgetedExpenses;
        UnspentBalance = TotalBudgetedExpenses - TotalExpended;
        UnbudgetedExpenses = categorySnapshots.FirstOrDefault(
            snapshot => string.Equals(
            snapshot.GeneralClassification,
            CategorySnapshot.UnbudgetedClassification,
            StringComparison.OrdinalIgnoreCase))
            ?.ActualAmount ?? 0;
        SourceFileName = sourceFileName;
    }
}