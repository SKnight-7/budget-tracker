using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Assembles the financial picture: takes the budgets and the transactions,
/// hands back a FinancialSnapshot. Stateless; every answer is computed
/// fresh from whatever the caller passes in.
/// </summary>
public static class SnapshotGenerator
{
    /// <summary>Builds one CategorySnapshot per budget category, plus one
    /// appended row holding all unbudgeted money.</summary>
    /// <remarks>Each category's actual amount is the sum of its
    /// transactions' amounts, or zero when it has none. The sign of that
    /// sum is flipped for every non-income category, so spending reads as
    /// a positive number even though bank convention records it as
    /// negative. Transactions whose category matches no tracked budget are
    /// combined into one extra row, added after the category rows. That
    /// row's GeneralClassification is CategorySnapshot.UnbudgetedClassification,
    /// its CategoryName is Transaction.UnbudgetedCategoryName, its
    /// BudgetedAmount is zero, and its ActualAmount is the sum of those
    /// transactions' amounts with the sign flipped. Category matching is
    /// case-insensitive throughout.</remarks>
    /// <returns>A fresh FinancialSnapshot carrying the rows, the overview
    /// figures, and the batch's source file name. Never null.</returns>
    public static FinancialSnapshot GenerateSnapshot(BudgetSet categories, TransactionBatch transactionBatch)
    {
        Dictionary<string, List<Transaction>> groupedTransactions = TransactionGrouper.GroupByCategory(transactionBatch);

        List<CategorySnapshot> snapshots = [];

        foreach (BudgetCategory category in categories.Categories)
        {
            decimal actualAmount = 0;
            if (groupedTransactions.ContainsKey(category.Name))
                actualAmount = groupedTransactions[category.Name].Sum(transaction => transaction.Amount);

            if (!string.Equals(category.GeneralClassification, BudgetCategory.IncomeClassification, StringComparison.OrdinalIgnoreCase))
                actualAmount = -actualAmount;

            snapshots.Add(
                new CategorySnapshot(
                    category.GeneralClassification,
                    category.Name,
                    category.BudgetedAmount,
                    actualAmount)
                );

            groupedTransactions.Remove(category.Name);
        }

        decimal unbudgetedAmount = -groupedTransactions.Values
            .Sum(group => group.Sum(transaction => transaction.Amount));

        snapshots.Add(
            new CategorySnapshot(
                CategorySnapshot.UnbudgetedClassification,
                Transaction.UnbudgetedCategoryName,
                0,
                unbudgetedAmount
            )
        );

        return new FinancialSnapshot(snapshots, transactionBatch.SourceFileName);
    }
}