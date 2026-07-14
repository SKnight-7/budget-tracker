using System.Diagnostics.CodeAnalysis;
namespace BudgetTracker.Models;

public class Transaction
{
    public Transaction(int number, DateOnly date,
                       decimal amount, string description,
                       string category = "Uncategorized", string sourceFile = "sample.csv")
    {
        Number = number;
        Date = date;
        Amount = amount;
        Description = description;
        Category = category;
        SourceFile = sourceFile;
    }

    private int _number;
    public int Number
    {
        get => _number;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Number));
            _number = value;
        }
    }

    public DateOnly Date { get; set; }
    private decimal _amount;

    public decimal Amount
    {
        get => _amount;
        set => _amount = Math.Round(value, 2);
    }
    public string Description { get; set; }
    public string Category { get; set; }
    private string _sourceFile;

    public string SourceFile
    {
        get => _sourceFile;

        [MemberNotNull(nameof(_sourceFile))]
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            if (!value.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Source file must be a CSV file.", nameof(SourceFile));
            _sourceFile = value;
        }
    }

    public override string ToString() =>
        $"""
        Transaction Number: {Number}
        Transaction Date: {Date}
        Transaction Amount: {Amount:C}
        Description: {Description}
        Category: {Category}
        Source File: {SourceFile}
        """;
    
}