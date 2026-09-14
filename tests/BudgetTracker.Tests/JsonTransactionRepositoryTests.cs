using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for JsonTransactionRepository: what Load returns for a missing, empty,
/// or damaged file, and that Save followed by Load gives back what was saved,
/// source file name included. The repository is pointed at its own test file
/// name, so the app's real transactions file is never read or written by
/// these tests.
/// </summary>
public class JsonTransactionRepositoryTests : IDisposable
{
    private const string TestFileName = "testTransactions.json";
    private const string SourceFileName = "august.csv";

    private readonly JsonTransactionRepository _repository = new(TestFileName);

    private static string TestFilePath => Path.Combine(FolderPaths.StatePersistence, TestFileName);

    // Runs before each test: guards against a leftover file from a test run
    // that was killed partway through.
    public JsonTransactionRepositoryTests()
    {
        File.Delete(TestFilePath);
    }

    // xUnit calls Dispose after each test, so no test leaves its file behind.
    public void Dispose()
    {
        File.Delete(TestFilePath);
    }

    // The amount has three decimals on purpose, to show that rounding happens.
    // It deliberately avoids a midpoint such as -12.345, whose rounding
    // direction depends on a policy the app has not decided yet.
    private static Transaction MakeTransaction(int number) =>
        new(number, new DateOnly(2026, 8, 22), -12.346m, "SAFEWAY 1234", "Groceries");

    /// <summary>With no file stored yet, Load returns null.</summary>
    [Fact]
    public void Load_NoFile_ReturnsNull()
    {
        TransactionBatch? loaded = _repository.Load();

        Assert.Null(loaded);
    }

    /// <summary>What Save writes, Load reads back: the transactions and the
    /// batch's source file name. The amount comes back already rounded to two
    /// places, because the Transaction setter rounds on assignment.</summary>
    [Fact]
    public void SaveThenLoad_RoundTripsTheBatch()
    {
        TransactionBatch saved = new([MakeTransaction(1), MakeTransaction(2)], SourceFileName);

        _repository.Save(saved);
        TransactionBatch? loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(SourceFileName, loaded.SourceFileName);
        Assert.Equal(2, loaded.Transactions.Count);
        Assert.Equal(new DateOnly(2026, 8, 22), loaded.Transactions[0].Date);
        Assert.Equal(-12.35m, loaded.Transactions[0].Amount);
        Assert.Equal("Groceries", loaded.Transactions[0].Category);
        Assert.Equal(2, loaded.Transactions[1].Number);
    }

    /// <summary>Saving an empty batch writes a real file, but loading it gives
    /// null, the same as nothing stored.</summary>
    [Fact]
    public void SaveEmptyThenLoad_ReturnsNull()
    {
        _repository.Save(new([], SourceFileName));
        TransactionBatch? loaded = _repository.Load();

        Assert.True(File.Exists(TestFilePath));
        Assert.Null(loaded);
    }

    /// <summary>A file carrying two transactions with the same number is refused
    /// by the TransactionBatch constructor during deserialization, and the
    /// repository reports it as InvalidDataException with the constructor's
    /// error attached.</summary>
    [Fact]
    public void Load_DuplicateNumbersInFile_ThrowsInvalidDataException()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath, """
            {
              "Transactions": [
                { "Number": 7, "Date": "2026-08-22", "Amount": -1, "Description": "a", "Category": "Groceries" },
                { "Number": 7, "Date": "2026-08-23", "Amount": -2, "Description": "b", "Category": "Household" }
              ],
              "SourceFileName": "august.csv"
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
