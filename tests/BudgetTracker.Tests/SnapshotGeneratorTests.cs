using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for the SnapshotGenerator's arithmetic: the sign rules and the
/// unbudgeted handling. These rules don't announce their own failures on
/// screen (a flipped sign produces a tidy table of wrong numbers), so the
/// suite holds them instead of the eye. Each test uses tiny fixed amounts
/// whose expected totals can be checked in the head.
/// </summary>
public class SnapshotGeneratorTests
{
    // A small budget set: one income category and two expense categories,
    // with budgeted amounts chosen so overview figures are easy to verify
    // by hand. The keywords are deliberate placeholders: nothing in these
    // tests reads them, because the generator trusts the Category already
    // stamped on each transaction and never consults the categorizer.
    // xUnit creates a fresh instance of this class for every test, so no
    // test can contaminate another.
    private readonly BudgetSet _budgets = new(
    [
        new(BudgetCategory.IncomeClassification, "Paycheck", ["keyword1"], 1, 1000m, 1),
        new("Food & Dining", "Groceries", ["keyword2"], 2, 300m, 2),
        new("Other", "Pet Care", ["keyword3"], 3, 100m, 3),
    ]);

    // Wraps transactions in a batch with a fixed test source name.
    private static TransactionBatch MakeBatch(params Transaction[] transactions) =>
        new([.. transactions], "test.csv");

    /// <summary>Transactions whose category matches no tracked budget are
    /// combined into one unbudgeted amount, and never appear among the
    /// expense rows.</summary>
    [Fact]
    public void GenerateSnapshot_UntrackedCategories_CombineIntoOneUnbudgetedAmount()
    {
        TransactionBatch batch = MakeBatch(
            new(1, new DateOnly(2026, 10, 1), -8.50m, "LEMONADE STAND", "Lemonade"),
            new(2, new DateOnly(2026, 10, 2), -42.75m, "POTTERY STUDIO", "Pottery"));

        FinancialSnapshot snapshot = SnapshotGenerator.GenerateSnapshot(_budgets, batch);

        Assert.Equal(51.25m, snapshot.UnbudgetedExpenses);
        Assert.DoesNotContain(snapshot.ExpenseRows, row => row.CategoryName == "Lemonade");
        Assert.DoesNotContain(snapshot.ExpenseRows, row => row.CategoryName == "Pottery");
    }

    /// <summary>Unbudgeted money never enters the overview figures: the
    /// totals compare the plan only against money the plan covers.</summary>
    [Fact]
    public void GenerateSnapshot_UnbudgetedMoney_ExcludedFromOverviewFigures()
    {
        TransactionBatch batch = MakeBatch(
            new(1, new DateOnly(2026, 10, 3), -50.00m, "GROCERY OUTLET", "Groceries"),
            new(2, new DateOnly(2026, 10, 4), -999.00m, "MYSTERY MACHINE", "Mystery"));

        FinancialSnapshot snapshot = SnapshotGenerator.GenerateSnapshot(_budgets, batch);

        Assert.Equal(50.00m, snapshot.TotalExpended);        // the 999 is not here
        Assert.Equal(400.00m, snapshot.TotalBudgetedExpenses); // 300 + 100, unbudgeted adds nothing
        Assert.Equal(350.00m, snapshot.UnspentBalance);        // 400 - 50
        Assert.Equal(999.00m, snapshot.UnbudgetedExpenses);    // reported beside the plan, not in it
    }

    /// <summary>Income amounts keep their bank signs: positive arrivals
    /// and negative corrections both pass through unflipped.</summary>
    [Fact]
    public void GenerateSnapshot_IncomeSign_UnchangedFromTransactions()
    {
        TransactionBatch batch = MakeBatch(
            new(1, new DateOnly(2026, 10, 3), 4400.00m, "Regular Payroll", "Paycheck"),
            new(2, new DateOnly(2026, 10, 4), 150.00m, "Supplemental Stipend", "Paycheck"),
            new(3, new DateOnly(2026, 10, 4), -50.00m, "Overpayment", "Paycheck"));

        FinancialSnapshot snapshot = SnapshotGenerator.GenerateSnapshot(_budgets, batch);

        Assert.Equal(4500.00m, snapshot.TotalReceived);
    }

    /// <summary>Expense amounts display with flipped signs, refunds
    /// included, and the same rule covers unbudgeted spending: an expense
    /// is an expense wherever it lands.</summary>
    [Fact]
    public void GenerateSnapshot_ExpenseSign_OppositeOfTransactions()
    {
        TransactionBatch batch = MakeBatch(
            new(1, new DateOnly(2026, 10, 3), -200.00m, "GROCERY OUTLET", "Groceries"),
            new(2, new DateOnly(2026, 10, 4), -550.00m, "MIDTOWN ANIMAL HOSPITAL", "Pet Care"),
            new(3, new DateOnly(2026, 10, 4), 50.00m, "RETURNED ITEM", "Groceries"),
            new(4, new DateOnly(2026, 10, 7), -100.00m, "MYSTERY EXPENSE", "Mystery"),
            new(5, new DateOnly(2026, 10, 8), 25.00m, "MYSTERY RETURN", "Mystery"));

        FinancialSnapshot snapshot = SnapshotGenerator.GenerateSnapshot(_budgets, batch);

        Assert.Equal(700.00m, snapshot.TotalExpended);
        Assert.Equal(75.00m, snapshot.UnbudgetedExpenses);
    }
}
