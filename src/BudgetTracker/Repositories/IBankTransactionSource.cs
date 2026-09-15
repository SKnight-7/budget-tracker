using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// The contract for reading bank transactions. Only fetches the source transactions
/// because these files will never be written to. Managers call the interface, never relying on a
/// fixed format, so the details can change according to how each bank formats its output
/// without any code changes to the callers.
/// </summary>
public interface IBankTransactionSource
{
    /// <summary>Fetches the original transactions
    /// and carries the name of the file they came from.</summary>
    /// <param name="fileName">The name of the bank file to read.</param>
    /// <returns>The fetched batch, or null when the file is not found;
    /// what a missing file means is the caller's decision.</returns>
    TransactionBatch? Load(string fileName);
}
