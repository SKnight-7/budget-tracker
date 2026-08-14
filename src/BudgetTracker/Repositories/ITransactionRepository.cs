namespace BudgetTracker.Repositories;

/// <summary>
/// The contract for transaction storage: fetching the stored transactions
/// and storing new ones. Managers call the interface, never a specific
/// storage class, so the storage format can change without any code changes
/// to the callers.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Fetches the stored batch: the transactions and the name of
    /// the bank file they came from.</summary>
    /// <returns>The stored batch, or null when nothing is stored yet;
    /// what an empty start means is the caller's decision, not storage's.</returns>
    TransactionBatch? Load();

    /// <summary>Stores the given batch, replacing whatever was stored before.</summary>
    /// <param name="batch">The transactions to store, together with the name
    /// of the bank file they came from.</param>
    void Save(TransactionBatch batch);
}