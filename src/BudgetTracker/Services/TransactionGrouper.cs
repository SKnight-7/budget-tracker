using BudgetTracker.Models;

namespace BudgetTracker.Services;

public static class TransactionGrouper
{
    public static Dictionary<string, List<Transaction>> GroupByCategory(TransactionBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Transactions
        .GroupBy(transaction => transaction.Category, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
    }
}