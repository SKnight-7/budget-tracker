namespace BudgetTracker.Models;

/// <summary>
/// One validated set of budget categories: every budget the app is tracking,
/// guaranteed free of duplicate names. The budgets twin of
/// <see cref="TransactionBatch"/>.
/// </summary>
public class TrackedBudgets
{
    /// <summary>The budget categories in this set. Assigned at construction
    /// and never reassigned; guaranteed free of duplicate names, compared
    /// case-insensitively.</summary>
    public List<BudgetCategory> Categories { get; }

    /// <summary>
    /// All validation happens here, because construction is the only door:
    /// a set that would break the rules can never exist.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the category
    /// list is null. An empty list is allowed; tracking no budgets yet is
    /// a valid truth.</exception>
    /// <exception cref="ArgumentException">Thrown when two or more
    /// categories share a name, ignoring case, naming every duplicated name
    /// in one message. Each duplicate is reported in whichever spelling
    /// appeared first in the list.</exception>
    public TrackedBudgets(List<BudgetCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        List<string> duplicateNames = [.. categories
        .GroupBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)];

        if (duplicateNames.Count > 0)
            throw new ArgumentException(
                $"Category names must be unique; duplicated: {string.Join(", ", duplicateNames)}.",
                nameof(categories));

        Categories = categories;
    }
}