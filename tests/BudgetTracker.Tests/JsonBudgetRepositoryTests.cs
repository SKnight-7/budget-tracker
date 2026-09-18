using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for JsonBudgetRepository: what Load returns for a missing, empty, or
/// damaged file, and that Save followed by Load gives back what was saved.
/// The repository is pointed at its own test file name, so the app's real
/// budgets file is never read or written by these tests.
/// </summary>
public class JsonBudgetRepositoryTests : IDisposable
{
    private const string TestFileName = "testBudgets.json";

    private readonly JsonBudgetRepository _repository = new(TestFileName);

    // The same path the repository computes, so a test can plant file
    // contents by hand or confirm the file is gone.
    private static string TestFilePath => Path.Combine(FolderPaths.StatePersistence, TestFileName);

    // xUnit creates a fresh instance of this class for every test, so this
    // constructor runs before each one. Deleting the test file here guards
    // against a leftover from a test run that was killed partway through.
    public JsonBudgetRepositoryTests()
    {
        File.Delete(TestFilePath);   // does nothing if the file does not exist
    }

    // Because the class implements IDisposable, xUnit calls Dispose after each
    // test, whether it passed, failed, or threw. Deleting here means no test
    // leaves its file behind for the next one.
    public void Dispose()
    {
        File.Delete(TestFilePath);
    }

    private static BudgetCategory MakeCategory(string name, int optionNumber) =>
        new("Other", name, ["keyword"], optionNumber, 25.5m, optionNumber);

    /// <summary>With no file stored yet, Load returns null.</summary>
    [Fact]
    public void Load_NoFile_ReturnsNull()
    {
        BudgetSet? loaded = _repository.Load();

        Assert.Null(loaded);
    }

    /// <summary>What Save writes, Load reads back: same count, names, keywords,
    /// and amounts.</summary>
    [Fact]
    public void SaveThenLoad_RoundTripsTheCategories()
    {
        BudgetSet saved = new([MakeCategory("Groceries", 1), MakeCategory("Eating Out", 2)]);

        _repository.Save(saved);
        BudgetSet? loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Categories.Count);
        Assert.Equal("Groceries", loaded.Categories[0].Name);
        Assert.Equal("keyword", loaded.Categories[0].Keywords[0]);
        Assert.Equal(25.5m, loaded.Categories[0].AmountBudgeted);
        Assert.Equal("Eating Out", loaded.Categories[1].Name);
    }

    /// <summary>Saving an empty set writes a real file, but loading it gives
    /// null, the same as nothing stored, so the manager's defaults policy applies.</summary>
    [Fact]
    public void SaveEmptyThenLoad_ReturnsNull()
    {
        _repository.Save(new([]));
        BudgetSet? loaded = _repository.Load();

        Assert.True(File.Exists(TestFilePath));
        Assert.Null(loaded);
    }

    /// <summary>A file carrying two categories with the same name is refused by
    /// the BudgetSet constructor during deserialization, and the repository
    /// reports it as InvalidDataException with the constructor's error attached.</summary>
    [Fact]
    public void Load_DuplicateNamesInFile_ThrowsInvalidDataException()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath, """
            {
              "Categories": [
                { "GeneralClassification": "Food", "Name": "Groceries", "Keywords": ["a"], "OptionNumber": 1, "AmountBudgeted": 10, "SearchOrder": 1 },
                { "GeneralClassification": "Household", "Name": "groceries", "Keywords": ["b"], "OptionNumber": 2, "AmountBudgeted": 20, "SearchOrder": 2 }
              ]
            }
            """);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => _repository.Load());

        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    /// <summary>Text that is not JSON at all is reported as damage, not as an
    /// empty file.</summary>
    [Fact]
    public void Load_MalformedText_ThrowsInvalidDataException()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath, "{ this is not json");

        Assert.Throws<InvalidDataException>(() => _repository.Load());
    }
}
