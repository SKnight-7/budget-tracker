using BudgetTracker.Models;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for the BudgetSet model. Construction is the only door, so
/// every test is about what the constructor accepts and what it refuses.
/// Each test reads as a small specification of the model's intended behavior.
/// </summary>
public class BudgetSetTests
{
    // Builds one valid category with the given name. Everything except the
    // name is filler: these tests care about names only.
    private static BudgetCategory MakeCategory(string name, int optionNumber) =>
        new("Other", name, ["keyword"], optionNumber, 0m, optionNumber);

    /// <summary>A null category list is refused.</summary>
    [Fact]
    public void Constructor_NullList_ThrowsArgumentNullException()
    {
        // The parameter is declared non-nullable, so the compiler warns about
        // passing null. The "!" after null tells the compiler "I know, this is
        // deliberate": it changes nothing at runtime, it only silences the warning.
        Assert.Throws<ArgumentNullException>(() => new BudgetSet(null!));
    }

    /// <summary>An empty list is allowed: tracking no budgets yet is a valid truth.</summary>
    [Fact]
    public void Constructor_EmptyList_IsAllowed()
    {
        BudgetSet budgets = new([]);

        // Assert.Empty passes when the collection has no items.
        Assert.Empty(budgets.Categories);
    }

    /// <summary>Categories with distinct names are accepted, and the model keeps
    /// the very same list object it was given rather than a copy.</summary>
    [Fact]
    public void Constructor_UniqueNames_KeepsTheGivenList()
    {
        List<BudgetCategory> categories =
        [
            MakeCategory("Groceries", 1),
            MakeCategory("Eating Out", 2),
        ];

        BudgetSet budgets = new(categories);

        // Assert.Same checks that both names refer to one and the same object,
        // not merely two lists with equal contents.
        Assert.Same(categories, budgets.Categories);
    }

    /// <summary>Two categories whose names differ only in case are duplicates,
    /// and the error names the duplicate in the spelling that appeared first.</summary>
    [Fact]
    public void Constructor_DuplicateNamesDifferingInCase_ThrowsArgumentException()
    {
        List<BudgetCategory> categories =
        [
            MakeCategory("Groceries", 1),
            MakeCategory("GROCERIES", 2),
        ];

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new BudgetSet(categories));

        // Assert.Contains checks that the first string appears somewhere inside the second.
        Assert.Contains("Groceries", exception.Message);
    }
}
