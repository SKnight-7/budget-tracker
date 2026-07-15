using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Assigns budget categories to transactions by matching category keywords
/// against transaction descriptions.
/// </summary>
public static class Categorizer
{
    /// <summary>Finds the budget category whose keywords match a transaction description.</summary>
    /// <param name="categories">The categories to consider; they are checked in ascending
    /// SearchOrder so more specific categories get first claim.</param>
    /// <param name="description">The transaction description to match against.</param>
    /// <returns>The name of the first matching category, or "Uncategorized" when nothing matches.</returns>
    /// <remarks>Matching is case-insensitive substring containment against the description
    /// padded with a leading and trailing space, so keywords with deliberate edge spaces
    /// (" rt ") also match at the start and end of descriptions.</remarks>
    public static string Categorize(List<BudgetCategory> categories, string description)
    {
        string paddedDescription = $" {description} ";
        foreach (BudgetCategory category in categories.OrderBy(c => c.SearchOrder)) 
        {
            foreach (string keyword in category.Keywords)
            {
                if (paddedDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return category.Name;
            }
        }
        return "Uncategorized";
    }
}