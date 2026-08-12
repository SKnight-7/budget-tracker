using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// The contract for transaction storage: fetching the stored transactions
/// and storing new ones. Managers call the interface, never a specific
/// storage class, so the storage format can change without any code changes
/// to the callers.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Fetches the stored transactions.</summary>
    /// <returns>The stored transactions, or null when nothing is stored yet —
    /// what an empty start means is the caller's decision, not storage's.</returns>
    List<Transaction>? Load();

    /// <summary>Stores the given transactions, replacing whatever was stored before.</summary>
    /// <param name="transactions">The transactions to store.</param>
    void Save(List<Transaction> transactions);
}