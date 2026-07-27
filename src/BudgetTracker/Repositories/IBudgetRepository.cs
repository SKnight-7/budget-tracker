using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// The contract for budget storage: fetching the stored budget categories
/// and storing new ones. Managers call the interface, never a specific
/// storage class, so the storage format can change without any code changes
/// to the callers.
/// </summary>
public interface IBudgetRepository
{
    /// <summary>Fetches the stored budget categories.</summary>
    /// <returns>The stored categories, or null when nothing is stored yet —
    /// what an empty start means is the caller's decision, not storage's.</returns>
    List<BudgetCategory>? Load();

    /// <summary>Stores the given categories, replacing whatever was stored before.</summary>
    /// <param name="categories">The categories to store.</param>
    void Save(List<BudgetCategory> categories);
}
