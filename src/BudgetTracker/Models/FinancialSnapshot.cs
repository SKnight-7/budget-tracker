namespace BudgetTracker.Models;

public class FinancialSnapshot
{
    public decimal TotalExpectedIncome { get; }
    public decimal TotalReceived { get; }
    public decimal TotalBudgetedExpenses { get; }
    public decimal TotalExpended { get; }
    public decimal AvailableToAllocate { get; }
    public decimal UnspentBalance { get; }
    public decimal UnbudgetedExpenses { get; }

    public FinancialSnapshot(List<CategorySnapshot> categorySnapshots)
    {
        List<CategorySnapshot> incomeRows = [.. categorySnapshots
        .Where(snapshot => string.Equals(
            snapshot.GeneralClassification,
            BudgetCategory.IncomeClassification,
            StringComparison.OrdinalIgnoreCase))];

        List<CategorySnapshot> expenseRows = [.. categorySnapshots
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

        TotalExpectedIncome = incomeRows.Sum(snapshot => snapshot.BudgetedAmount);
        TotalReceived = incomeRows.Sum(snapshot => snapshot.ActualAmount);
        TotalBudgetedExpenses = expenseRows.Sum(snapshot => snapshot.BudgetedAmount);
        TotalExpended = expenseRows.Sum(snapshot => snapshot.ActualAmount);

        AvailableToAllocate = TotalExpectedIncome - TotalBudgetedExpenses;
        UnspentBalance = TotalBudgetedExpenses - TotalExpended;
        UnbudgetedExpenses = categorySnapshots.FirstOrDefault(
            snapshot => string.Equals(
            snapshot.GeneralClassification,
            CategorySnapshot.UnbudgetedClassification,
            StringComparison.OrdinalIgnoreCase))
            ?.ActualAmount ?? 0;
    }
}