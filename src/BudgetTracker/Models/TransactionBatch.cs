namespace BudgetTracker.Models;

/// <summary>
/// One saved set of transactions together with the name of the bank file
/// they came from. This is the single value the transaction repositories
/// trade in: Load returns one, Save receives one.
/// </summary>
public class TransactionBatch
{
    /// <summary>Recorded as the batch's source when the given name is missing
    /// or blank: an honest label, never a made-up file name.</summary>
    private const string UnknownSourceName = "unknown";

    /// <summary>The transactions in this batch. Assigned at construction and
    /// never reassigned; guaranteed free of duplicate transaction numbers.</summary>
    public List<Transaction> Transactions { get; }

    /// <summary>The name of the bank file the batch came from, or "unknown"
    /// when no name was given. Assigned at construction and never reassigned.</summary>
    public string SourceFileName { get; }

    /// <summary>
    /// All validation happens here, because construction is the only door:
    /// a batch that would break the rules can never exist.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the transaction
    /// list is null. An empty list is allowed; a quiet date range is a valid
    /// truth.</exception>
    /// <exception cref="ArgumentException">Thrown when two or more
    /// transactions share a number, naming every duplicated number in one
    /// message.</exception>
    public TransactionBatch(List<Transaction> transactions, string sourceFileName)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        if (string.IsNullOrWhiteSpace(sourceFileName))
            sourceFileName = UnknownSourceName;

        List<int> duplicateNumbers = [.. transactions
            .GroupBy(transaction => transaction.Number)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)];

        if (duplicateNumbers.Count > 0)
            throw new ArgumentException(
                $"Transaction numbers must be unique; duplicated: {string.Join(", ", duplicateNumbers)}.",
                nameof(transactions));

        Transactions = transactions;
        SourceFileName = sourceFileName;
    }
}