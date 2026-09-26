using System.Diagnostics.CodeAnalysis;

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
    /// <summary>The classification value that marks a category as income;
    /// compared case-insensitively wherever the income test is made. Every
    /// other classification counts as an expense.</summary>
    public const string IncomeClassification = "Income";

    private string _generalClassification;

    /// <summary>The broad grouping the category belongs to. Categories classified
    /// as "Income" count as money in; all others count as money out. Never null:
    /// a missing or whitespace classification becomes the empty string, an
    /// unlabeled-but-valid state that counts as money out.</summary>
    public string GeneralClassification
    {
        get => _generalClassification;

        [MemberNotNull(nameof(_generalClassification))]
        set
        {
            _generalClassification = string.IsNullOrWhiteSpace(value) ? "" : value;
        }
    }

    private string _name;

    /// <summary>The specific thing being budgeted for, such as "Groceries" or "Paycheck".</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when set to an empty or
    /// whitespace name. There is no honest meaning for a budget about nothing,
    /// and every name-keyed structure downstream depends on this value.</exception>
    public string Name
    {
        get => _name;

        [MemberNotNull(nameof(_name))]
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Name));
            _name = value;
        }
    }

    private List<string> _keywords;

    /// <summary>Substrings the categorizer looks for in transaction descriptions.
    /// An empty list is allowed: a category with no keywords simply never matches.</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when any entry is empty or
    /// whitespace, because an empty keyword would match every description.</exception>
    public List<string> Keywords
    {
        get => _keywords;

        [MemberNotNull(nameof(_keywords))]
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Keywords must not be empty or whitespace.", nameof(Keywords));

            _keywords = value;
        }
    }

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

    private decimal _budgetedAmount;
    /// <summary>The amount budgeted for the category: at most two decimal places, never silently rounded.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative amount.</exception>
    /// <exception cref="ArgumentException">Thrown when set to a value with
    /// more than two decimal places. Typed budget amounts are the
    /// interaction layer's to round, deliberately and visibly.</exception>
    public decimal BudgetedAmount
    {
        get => _budgetedAmount;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(BudgetedAmount));

            if (value != Math.Round(value, 2))
                throw new ArgumentException(
                    $"Amounts must have at most two decimal places; got {value}.", nameof(BudgetedAmount));

            _budgetedAmount = value;
        }
    }

    /// <summary>The category's position in the categorization search; lower values are checked first.</summary>
    // Deliberately an unrestricted decimal (the Python original required a non-negative
    // integer) so categories can be reordered or prioritized without renumbering the list.
    public decimal SearchOrder { get; set; }

    /// <summary>
    /// The constructor assigns through the properties, so their range checks run
    /// during construction too: a category can't be created with values the
    /// setters would reject.
    /// </summary>
    public BudgetCategory(string generalClassification, string name, List<string> keywords,
                          int optionNumber, decimal budgetedAmount, decimal searchOrder)
    {
        GeneralClassification = generalClassification;
        Name = name;
        Keywords = keywords;
        OptionNumber = optionNumber;
        BudgetedAmount = budgetedAmount;
        SearchOrder = searchOrder;
    }

    /// <summary>Returns every property as labeled lines, for debugging and quick prints.</summary>
    public override string ToString() =>
        $"""
        General Classification: {GeneralClassification}
        Budget Category: {Name}
        Keywords: {string.Join(", ", Keywords)}
        Option Number: {OptionNumber}
        Budgeted Amount: {BudgetedAmount:C}
        Search Order: {SearchOrder}
        """;
}