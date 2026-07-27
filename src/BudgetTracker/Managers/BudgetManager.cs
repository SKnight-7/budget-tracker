using BudgetTracker.Defaults;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Managers;

/// <summary>
/// Holds the live budget state for a session: every category keyed by its name.
/// </summary>
public class BudgetManager
{
    private readonly IBudgetRepository _repository;

    /// <summary>Every budget category, fetchable by its name. Each BudgetCategory
    /// carries: GeneralClassification, Name, Keywords, OptionNumber, AmountBudgeted,
    /// and SearchOrder — documented in full on the class itself.</summary>
    public Dictionary<string, BudgetCategory> BudgetCategories { get; set; }

    /// <summary>
    /// A manager receives its repository through the constructor; the
    /// repository can be any class implementing IBudgetRepository, and the
    /// manager only ever calls the interface's methods, so no storage-format
    /// code appears anywhere in this class. It starts from the default
    /// categories, keyed by name.
    /// </summary>
    public BudgetManager(IBudgetRepository repository)
    {
        _repository = repository;
        BudgetCategories = DefaultCategories.GetDefaults().ToDictionary(category => category.Name);
    }

    /// <summary>Hands every category currently in memory to the repository for storage.</summary>
    public void SaveBudgets() =>
        _repository.Save([.. BudgetCategories.Values]);

    /// <summary>
    /// Replaces the in-memory categories with whatever the repository has
    /// stored. When the repository has nothing yet, the categories already in
    /// memory — the defaults, on a fresh start — are saved instead, seeding
    /// storage: when disk and memory disagree about existing, memory is the
    /// survivor.
    /// </summary>
    /// <remarks>Anything the repository throws while reading — a rejected row,
    /// damaged data — travels up through this method unchanged.</remarks>
    public void LoadBudgets()
    {
        List<BudgetCategory>? loaded = _repository.Load();

        if (loaded is null)
        {
            SaveBudgets();
            return;
        }

        BudgetCategories = loaded.ToDictionary(category => category.Name);
    }
}