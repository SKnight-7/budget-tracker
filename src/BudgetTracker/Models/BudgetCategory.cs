namespace BudgetTracker.Models;

/// <summary>
/// A budget category and the data used to match bank transactions to it.
/// Search order exists to prevent miscategorization when keywords overlap:
/// "animal hospital" must match Pet Care before Medical ever sees it, and a
/// store named "outlet" must be checked against every other category before
/// falling through to Other Shopping. Lower values are searched first.
/// </summary>
public class BudgetCategory
{
    /// <summary>
    /// The constructor assigns through the properties, so their range checks run
    /// during construction too: a category can't be created with values the
    /// setters would reject.
    /// </summary>
    public BudgetCategory(string generalClassification, string name, string[] keywords,
                          int optionNumber, decimal amtBudgeted, decimal searchOrder)
    {
        GeneralClassification = generalClassification;
        Name = name;
        Keywords = keywords;
        OptionNumber = optionNumber;
        AmtBudgeted = amtBudgeted;
        SearchOrder = searchOrder;
    }

    /// <summary>The broad grouping the category belongs to. Categories classified
    /// as "Income" count as money in; all others count as money out.</summary>
    public string GeneralClassification { get; set; }

    /// <summary>The specific thing being budgeted for, such as "Groceries" or "Paycheck".</summary>
    public string Name { get; set; }

    /// <summary>Substrings the categorizer looks for in transaction descriptions.</summary>
    public string[] Keywords { get; set; }

    private int _optionNumber;
    /// <summary>The number a user types to select this category from a menu.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to zero or a negative number.</exception>
    public int OptionNumber
    {
        get => _optionNumber;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(OptionNumber));
            _optionNumber = value;
        }
    }

    private decimal _amtBudgeted;
    /// <summary>The amount budgeted for the category, rounded to two decimal places on assignment.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative amount.</exception>
    public decimal AmtBudgeted
    {
        get => _amtBudgeted;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(AmtBudgeted));
            _amtBudgeted = Math.Round(value, 2);
        }
    }

    /// <summary>The category's position in the categorization search; lower values are checked first.</summary>
    // Deliberately an unrestricted decimal (the Python original required a non-negative
    // integer) so categories can be reordered or prioritized without renumbering the list.
    public decimal SearchOrder { get; set; }

    /// <summary>Returns every property as labeled lines, for debugging and quick prints.</summary>
    public override string ToString() =>
        $"""
        General Classification: {GeneralClassification}
        Budget Category: {Name}
        Keywords: {string.Join(", ", Keywords)}
        Option Number: {OptionNumber}
        Amount Budgeted: {AmtBudgeted:C}
        Search Order: {SearchOrder}
        """;
}