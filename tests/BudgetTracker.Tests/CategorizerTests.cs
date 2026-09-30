using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for the Categorizer service. Verifies that Categorize correctly maps
/// transaction descriptions to budget categories using keyword matching, with
/// priority resolved by SearchOrder (lower numbers checked first, ensuring the
/// right category wins when multiple keywords could match one description).
/// Each test reads as a small specification of the service's intended behavior.
/// </summary>
public class CategorizerTests
{
    // A small representative set of categories that exercises the SearchOrder
    // mechanism. The keywords and orders are chosen specifically to let several
    // descriptions potentially match more than one category, so the tests can
    // confirm that SearchOrder resolves the conflict correctly.
    //
    // xUnit creates a fresh instance of this class for every test, so each test
    // gets its own new copy of this set, so no test can contaminate another.
    private readonly BudgetSet _budgets = new(
    [
        new("Food & Dining", "Groceries", ["safeway", "grocery"], 1, 0m, 7),
        new("Food & Dining", "Eating Out", ["starbuck", "chipotle"], 2, 0m, 8),
        new("Health & Fitness", "Medical", ["hospital", "kaiser"], 3, 0m, 17),
        new("Other", "Pet Care", ["animal", "vet", "dog"], 4, 0m, 12),
        new("Other", "Entertainment", ["netflix", "video"], 5, 0m, 20),
        new("Shopping", "Other Shopping", ["amazon", "outlet", "target"], 6, 0m, 999),
        new("Travel & Transport", "Public Transit", ["transit", " rt "], 10, 0m, 10),
    ]);

    /// <summary>A description containing a clear keyword returns the matching category.</summary>
    [Fact]
    public void Categorize_DescriptionContainsKeyword_ReturnsMatchingCategory()
    {
        string result = Categorizer.Categorize(_budgets, "SAFEWAY 1234 SACRAMENTO");

        Assert.Equal("Groceries", result);
    }

    /// <summary>
    /// Matching ignores case in both description and keyword.
    /// </summary>
    [Fact]
    public void Categorize_MatchIsCaseInsensitive_AllCasesSameResult()
    {
        string lowercase = Categorizer.Categorize(_budgets, "starbucks downtown");
        string uppercase = Categorizer.Categorize(_budgets, "STARBUCKS DOWNTOWN");
        string mixedCase = Categorizer.Categorize(_budgets, "StArBuCks DoWnToWn");

        Assert.Equal("Eating Out", lowercase);
        Assert.Equal("Eating Out", uppercase);
        Assert.Equal("Eating Out", mixedCase);
    }

    /// <summary>
    /// An "animal hospital" description should match Pet Care (SearchOrder 12)
    /// before Medical (SearchOrder 17), even though "hospital" is a Medical keyword.
    /// </summary>
    [Fact]
    public void Categorize_SearchOrder_PetBeatsMedical()
    {
        string result = Categorizer.Categorize(_budgets, "ANIMAL HOSPITAL EMERGENCY VISIT");

        Assert.Equal("Pet Care", result);
    }

    /// <summary>
    /// A "grocery outlet" description should match Groceries (SearchOrder 7)
    /// before Other Shopping (SearchOrder 999), even though "outlet" is an
    /// Other Shopping keyword.
    /// </summary>
    [Fact]
    public void Categorize_SearchOrder_GroceriesBeatsOtherShopping()
    {
        string result = Categorizer.Categorize(_budgets, "GROCERY OUTLET 0987 SACRAMENTO");

        Assert.Equal("Groceries", result);
    }

    /// <summary>
    /// An "amazon prime video" description should match Entertainment
    /// (SearchOrder 20) before Other Shopping (SearchOrder 999), because
    /// "video" is an Entertainment keyword.
    /// </summary>
    [Fact]
    public void Categorize_SearchOrder_EntertainmentBeatsOtherShopping()
    {
        string result = Categorizer.Categorize(_budgets, "AMAZON PRIME VIDEO ONLINE");

        Assert.Equal("Entertainment", result);
    }

    /// <summary>
    /// A description containing no recognized keywords returns "Unbudgeted".
    /// </summary>
    [Fact]
    public void Categorize_NoMatch_ReturnsUnbudgeted()
    {
        string result = Categorizer.Categorize(_budgets, "CHECK # 1234");

        Assert.Equal("Unbudgeted", result);
    }

    /// <summary>
    /// An empty description returns "Unbudgeted".
    /// </summary>
    [Fact]
    public void Categorize_EmptyDescription_ReturnsUnbudgeted()
    {
        string result = Categorizer.Categorize(_budgets, "");

        Assert.Equal("Unbudgeted", result);
    }

    /// <summary>
    /// An empty category list returns "Unbudgeted" regardless of the description.
    /// </summary>
    [Fact]
    public void Categorize_EmptyCategories_ReturnsUnbudgeted()
    {
        string result = Categorizer.Categorize(new([]), "SAFEWAY GROCERIES");

        Assert.Equal("Unbudgeted", result);
    }

    /// <summary>
    /// When a description matches keywords from multiple categories, the category
    /// with the lower SearchOrder is chosen, even if the other category's keyword
    /// appears earlier in the description text.
    /// </summary>
    [Fact]
    public void Categorize_MultipleMatches_SearchOrderDetermines()
    {
        string result = Categorizer.Categorize(_budgets, "amazon refund video purchase");

        Assert.Equal("Entertainment", result);
    }

    [Fact]
    public void Categorize_KeywordMatch_KeywordsWithSpacesAtEndsMatchCorrectly()
    {
        string spaceAtBeginning = Categorizer.Categorize(_budgets, "RT MONTHLY PASS");
        string spaceAtEnd = Categorizer.Categorize(_budgets, "TRANSIT PASS SAC RT");

        Assert.Equal("Public Transit", spaceAtBeginning);
        Assert.Equal("Public Transit", spaceAtEnd);
    }
}
