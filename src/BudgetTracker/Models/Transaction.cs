namespace BudgetTracker.Models;

/// <summary>
/// A single bank transaction from an uploaded CSV file. Amounts keep the bank's
/// sign convention: negative for money out, positive for money in.
/// </summary>
public class Transaction
{
    private int _number;
    /// <summary>The transaction's position in its upload, numbered from 1. Zero is
    /// reserved for the stored-data placeholder, never a real transaction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative number.</exception>
    public int Number
    {
        get => _number;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Number));
            _number = value;
        }
    }

    /// <summary>The date the transaction occurred, as recorded by the bank.</summary>
    public DateOnly Date { get; set; }

    private decimal _amount;
    /// <summary>The transaction amount, rounded to two decimal places on assignment.
    /// Negative is money out, positive is money in, matching bank-CSV convention.</summary>
    public decimal Amount
    {
        get => _amount;
        set => _amount = Math.Round(value, 2);
    }
    /// <summary>The bank's description of the transaction; the categorizer matches
    /// its keywords against this text.</summary>
    public string Description { get; set; }

    /// <summary>The budget category the transaction is assigned to. Starts as
    /// "Uncategorized" until the categorizer or the user says otherwise.</summary>
    public string Category { get; set; }

    /// <summary>
    /// The constructor assigns through the properties, so their checks run during
    /// construction too: a transaction can't be created with values the setters
    /// would reject.
    /// </summary>
    public Transaction(int number, DateOnly date,
                       decimal amount, string description,
                       string category = "Uncategorized")
    {
        Number = number;
        Date = date;
        Amount = amount;
        Description = description;
        Category = category;
    }

    /// <summary>Returns every property as labeled lines, for debugging and quick prints.</summary>
    public override string ToString() =>
        $"""
        Transaction Number: {Number}
        Transaction Date: {Date}
        Transaction Amount: {Amount:C}
        Description: {Description}
        Category: {Category}
        """;
}