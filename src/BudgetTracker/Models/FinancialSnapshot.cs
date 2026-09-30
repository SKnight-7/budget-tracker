namespace BudgetTracker.Models;

/// <summary>
/// The whole financial picture at one moment: every category row plus the
/// overview figures, everything the budgets display needs in one object.
/// Built fresh by SnapshotGenerator on every request; no repository reads
/// or writes this type, and no class keeps an instance, so the numbers a
/// caller holds are only ever as old as the call that produced them.
/// </summary>
public class FinancialSnapshot
{
    /// <summary>The rows whose classification is Income, compared
    /// case-insensitively. Never null; empty when no income category
    /// survived the filter.</summary>
    public List<CategorySnapshot> IncomeRows { get; }

    /// <summary>The rows that are neither Income nor the unbudgeted row,
    /// compared case-insensitively. Never null; empty when no expense
    /// category survived the filter.</summary>
    public List<CategorySnapshot> ExpenseRows { get; }

    /// <summary>The sum of the income rows' budgeted amounts: what was
    /// planned to arrive.</summary>
    public decimal TotalExpectedIncome { get; }

    /// <summary>The sum of the income rows' actual amounts: what actually
    /// arrived.</summary>
    public decimal TotalReceived { get; }

    /// <summary>The sum of the expense rows' budgeted amounts: what was
    /// planned to be spent. The unbudgeted row does not count here.</summary>
    public decimal TotalBudgetedExpenses { get; }

    /// <summary>The sum of the expense rows' actual amounts: what was
    /// actually spent against the plan. The unbudgeted row does not count
    /// here, so money outside the plan never enters the plan's arithmetic.</summary>
    public decimal TotalExpended { get; }

    /// <summary>TotalExpectedIncome minus TotalBudgetedExpenses: expected
    /// income not yet assigned to any expense budget.</summary>
    public decimal AvailableToAllocate { get; }

    /// <summary>TotalBudgetedExpenses minus TotalExpended: budgeted money
    /// not yet spent.</summary>
    public decimal UnspentBalance { get; }

    /// <summary>The unbudgeted row's actual amount: net spending on
    /// categories no budget tracks, displayed beside the plan's totals but
    /// never counted in them. Zero when the given list has no unbudgeted
    /// row at all.</summary>
    public decimal UnbudgetedExpenses { get; }

    /// <summary>The name of the bank file the numbers are based on. Never
    /// blank when the snapshot came from SnapshotGenerator, because
    /// TransactionBatch records a missing name as "unknown".</summary>
    public string SourceFileName { get; }

    /// <summary>
    /// Splits the given rows into the income and expense lists and computes
    /// every overview figure from them, so the figures cannot disagree with
    /// the rows for any object that exists. The constructor does not check
    /// its arguments: every present caller passes values built from
    /// already-validated model objects.
    /// </summary>
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