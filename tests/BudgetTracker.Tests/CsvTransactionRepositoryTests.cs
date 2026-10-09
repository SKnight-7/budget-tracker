using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for CsvTransactionRepository, centered on the
/// collect-and-report-all behavior: a hand-edited file with several
/// problems gets them all named in one message, so one fixing pass covers
/// them. The repository is pointed at its own test file name, so the
/// app's real transactions file is never read or written by these tests.
/// </summary>
public class CsvTransactionRepositoryTests : IDisposable
{
    private const string TestFileName = "testTransactions.csv";

    private readonly CsvTransactionRepository _repository = new(TestFileName);

    private static string TestFilePath => Path.Combine(FolderPaths.StatePersistence, TestFileName);

    public CsvTransactionRepositoryTests()
    {
        File.Delete(TestFilePath);   // does nothing if the file does not exist
    }

    public void Dispose()
    {
        File.Delete(TestFilePath);
    }

    /// <summary>What Save writes, Load reads back, in transaction number
    /// order regardless of the order the batch arrived in.</summary>
    [Fact]
    public void SaveThenLoad_RoundTripsTheBatchInNumberOrder()
    {
        TransactionBatch saved = new(
        [
            new(2, new DateOnly(2026, 10, 2), -12.34m, "COFFEE", "Eating Out"),
            new(1, new DateOnly(2026, 10, 1), 2200.00m, "PAYROLL", "Paycheck"),
        ], "bank.csv");

        _repository.Save(saved);
        TransactionBatch? loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Transactions.Count);
        Assert.Equal(1, loaded.Transactions[0].Number);
        Assert.Equal(2, loaded.Transactions[1].Number);
        Assert.Equal("bank.csv", loaded.SourceFileName);
    }

    /// <summary>A file with several bad values reports every bad row in one
    /// message, instead of stopping at the first.</summary>
    [Fact]
    public void Load_SeveralBadValues_ReportsEveryBadRowTogether()
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);
        File.WriteAllText(TestFilePath,
            "Number,Date,Amount,Description,Category,SourceFileName\n" +
            "1,2026-10-01,12.34,COFFEE,Eating Out,bank.csv\n" +
            "2,2026-10-02,abc,LUNCH,Eating Out,bank.csv\n" +     // row 3: word where the amount belongs
            "3,not-a-date,5.00,SNACK,Eating Out,bank.csv\n");    // row 4: unreadable date

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => _repository.Load());

        Assert.Contains("row 3", exception.Message);
        Assert.Contains("row 4", exception.Message);
    }
}
