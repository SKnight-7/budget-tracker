using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Groups a batch's transactions by their category names. Stateless, like
/// the rest of the Services shelf: nothing is remembered between calls.
/// </summary>
public static class TransactionGrouper
{
    /// <summary>Files every transaction in the batch under its category name.</summary>
    /// <returns>A new dictionary on every call, keyed case-insensitively.
    /// The dictionary has one key for each category name that appears on
    /// at least one of the batch's transactions, and no other keys, so a
    /// category name that appears on no transaction is not in the
    /// dictionary; no key ever maps to an empty list. The lists hold the
    /// batch's own Transaction objects, not copies. The dictionary itself
    /// is not held by this class, and the caller may modify it.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the batch is null.</exception>
    public static Dictionary<string, List<Transaction>> GroupByCategory(TransactionBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Transactions
        .GroupBy(transaction => transaction.Category, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
    }
}