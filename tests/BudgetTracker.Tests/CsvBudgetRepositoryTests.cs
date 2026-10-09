using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for CsvBudgetRepository, centered on the collect-and-report-all
/// behavior: a hand-edited file with several problems gets them all named
/// in one message, so one fixing pass covers them. The repository is
/// pointed at its own test file name, so the app's real budgets file is
/// never read or written by these tests.
/// </summary>
public class CsvBudgetRepositoryTests : IDisposable
{
    private const string TestFileName = "testBudgets.csv";

    private readonly CsvBudgetRepository _repository = new(TestFileName);

    private static string TestFilePath => Path.Combine(FolderPaths.StatePersistence, TestFileName);

    public CsvBudgetRepositoryTests()
    {
        File.Delete(TestFilePath);   // does nothing if the file does not exist
    }

    public void Dispose()
    {
        File.Delete(TestFilePath);
    }

    /// <summary>What Save writes, Load reads back.</summary>
    [Fact]
    public void SaveThenLoad_RoundTripsTheCategories()
    {
        BudgetSet saved = new(
        [
            new("Food", "Groceries", ["grocery"], 1, 100.00m, 1),
            new("Other", "Pet Care", ["vet"], 2, 25.50m, 2),
        ]);

        _repository.Save(saved);
        BudgetSet? loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Categories.Count);
        Assert.Equal("Groceries", loaded.Categories[0].Name);
        Assert.Equal(25.50m, loaded.Categories[1].BudgetedAmount);
    }

    /// <summary>A file with several bad values reports every bad row in one
    /// message, instead of stopping at the first.</summary>
    [Fact]
    public void Load_SeveralBadValues_ReportsEveryBadRowTogether()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath,
            "GeneralClassification,Name,Keywords,OptionNumber,BudgetedAmount,SearchOrder\n" +
            "Food,Groceries,grocery,1,100.00,1\n" +
            "Food,Eating Out,taco,2,-50.00,2\n" +      // row 3: negative amount (model refuses)
            "Other,Pet Care,vet,abc,25.00,3\n");       // row 4: word where a number belongs

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => _repository.Load());

        Assert.Contains("row 3", exception.Message);
        Assert.Contains("row 4", exception.Message);
    }

    /// <summary>Duplicate names and bad values arrive in the same single
    /// report: one fixing pass covers problems of both kinds.</summary>
    [Fact]
    public void Load_DuplicateNameAndBadValue_ReportsBothInOneMessage()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath,
            "GeneralClassification,Name,Keywords,OptionNumber,BudgetedAmount,SearchOrder\n" +
            "Food,Groceries,a,1,10.00,1\n" +
            "Household,groceries,b,2,20.00,2\n" +      // duplicate of row 2's name
            "Other,Pets,vet,xyz,5.00,3\n");            // row 4: word where a number belongs

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => _repository.Load());

        Assert.Contains("'Groceries'", exception.Message);
        Assert.Contains("row 4", exception.Message);
    }
}
