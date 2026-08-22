using BudgetTracker.Defaults;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Managers;

/// <summary>
/// Holds the live budget state for a session: the categories being tracked.
/// </summary>
public class BudgetManager
{
    private readonly IBudgetRepository _repository;

    /// <summary>Every budget category. Each BudgetCategory carries:
    /// GeneralClassification, Name, Keywords, OptionNumber, AmountBudgeted,
    /// and SearchOrder, all documented in full on the class itself.</summary>
    public TrackedBudgets BudgetCategories { get; private set; }

    /// <summary>
    /// A manager receives its repository through the constructor; the
    /// repository can be any class implementing IBudgetRepository, and the
    /// manager only ever calls the interface's methods, so no storage-format
    /// code appears anywhere in this class. It starts from the default
    /// categories.
    /// </summary>
    public BudgetManager(IBudgetRepository repository)
    {
        _repository = repository;
        List<BudgetCategory> defaults = DefaultCategories.GetDefaults();

        BudgetCategories = new(defaults);
    }

    /// <summary>Hands every category currently in memory to the repository for storage.</summary>
    public void SaveBudgets() =>
        _repository.Save(BudgetCategories);

    /// <summary>
    /// Replaces the in-memory categories with whatever the repository has
    /// stored. When the repository has nothing yet, the categories already in
    /// memory (the defaults, on a fresh start) are saved instead, seeding
    /// storage: when disk and memory disagree about existing, memory is the
    /// survivor.
    /// </summary>
    /// <remarks>Anything the repository throws while reading, such as a rejected
    /// row or damaged data, travels up through this method unchanged.</remarks>
    public void LoadBudgets()
    {
        TrackedBudgets? loaded = _repository.Load();

        if (loaded is null)
        {
            SaveBudgets();
            return;
        }

        BudgetCategories = loaded;
    }

    /// <summary>Finds the category a user selected by menu number.</summary>
    /// <returns>The matching category, or null when no category has that number;
    /// the caller decides what a miss means (typically: re-prompt).</returns>
    public BudgetCategory? FindByOptionNumber(int optionNumber) =>
        BudgetCategories.Categories.FirstOrDefault(category => category.OptionNumber == optionNumber);

    /// <summary>Sets a category's budgeted amount and persists all budgets in
    /// one call, so no budget change can ever exist unsaved.</summary>
    /// <param name="category">The category to update. Must be one of this manager's
    /// own categories, typically obtained from FindByOptionNumber; the guard checks
    /// object identity, not name.</param>
    /// <param name="amount">The new budgeted amount; the model's own setter
    /// rejects negatives.</param>
    /// <exception cref="ArgumentException">Thrown when the given category is not
    /// one of this manager's categories.</exception>
    public void UpdateAmount(BudgetCategory category, decimal amount)
    {
        if (!BudgetCategories.Categories.Contains(category))
            throw new ArgumentException($"The given category ('{category.Name}') is not one of this manager's categories.", nameof(category));

        category.AmountBudgeted = amount;
        SaveBudgets();
    }
}