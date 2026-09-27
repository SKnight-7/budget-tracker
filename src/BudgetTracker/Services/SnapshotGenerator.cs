using BudgetTracker.Models;

namespace BudgetTracker.Services;

public static class SnapshotGenerator
{
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