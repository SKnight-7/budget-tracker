using BudgetTracker.Models;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests for the TransactionBatch model. Construction is the only door, so
/// every test is about what the constructor accepts, what it refuses, and
/// how it labels a batch with no source file name.
/// </summary>
public class TransactionBatchTests
{
    private const string SourceFileName = "august.csv";

    // Builds one valid transaction with the given number. Everything except
    // the number is filler: these tests care about numbers only.
    private static Transaction MakeTransaction(int number) =>
        new(number, new DateOnly(2026, 8, 22), -10m, "description");

    /// <summary>A null transaction list is refused.</summary>
    [Fact]
    public void Constructor_NullList_ThrowsArgumentNullException()
    {
        // The "!" after null silences the compiler's nullability warning; it
        // changes nothing at runtime.
        Assert.Throws<ArgumentNullException>(() => new TransactionBatch(null!, SourceFileName));
    }

    /// <summary>An empty list is allowed: a quiet date range is a valid truth,
    /// and the batch still carries its source file name.</summary>
    [Fact]
    public void Constructor_EmptyList_IsAllowedAndKeepsSource()
    {
        TransactionBatch batch = new([], SourceFileName);

        Assert.Empty(batch.Transactions);
        Assert.Equal(SourceFileName, batch.SourceFileName);
    }

    /// <summary>Transactions with distinct numbers are accepted, and the model
    /// keeps the very same list object it was given rather than a copy.</summary>
    [Fact]
    public void Constructor_UniqueNumbers_KeepsTheGivenList()
    {
        List<Transaction> transactions = [MakeTransaction(1), MakeTransaction(2)];

        TransactionBatch batch = new(transactions, SourceFileName);

        Assert.Same(transactions, batch.Transactions);
    }

    /// <summary>Two transactions sharing a number are duplicates, and the error
    /// names the duplicated number.</summary>
    [Fact]
    public void Constructor_DuplicateNumbers_ThrowsArgumentException()
    {
        List<Transaction> transactions = [MakeTransaction(7), MakeTransaction(7)];

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new TransactionBatch(transactions, SourceFileName));

        Assert.Contains("7", exception.Message);
    }

    /// <summary>A missing or blank source file name is recorded as "unknown",
    /// whether it arrives as null, empty, or whitespace.</summary>
    [Fact]
    public void Constructor_MissingSource_IsLabeledUnknown()
    {
        TransactionBatch fromNull = new([], null!);
        TransactionBatch fromEmpty = new([], "");
        TransactionBatch fromWhitespace = new([], "   ");

        Assert.Equal("unknown", fromNull.SourceFileName);
        Assert.Equal("unknown", fromEmpty.SourceFileName);
        Assert.Equal("unknown", fromWhitespace.SourceFileName);
    }
}
