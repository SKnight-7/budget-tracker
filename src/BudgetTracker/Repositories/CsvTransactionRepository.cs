using System.Diagnostics.CodeAnalysis;
using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using CsvHelper;
using System.Globalization;

namespace BudgetTracker.Repositories;

/// <summary>
/// The CSV implementation of <see cref="ITransactionRepository"/>: keeps the
/// transactions in a CSV file inside the StatePersistence folder.
/// </summary>
public class CsvTransactionRepository : ITransactionRepository
{
    /// <summary>The path to where the transactions file is stored on the
    /// local computer, computed from the state persistence folder and
    /// PersistenceFileName on every read, so the path can never fall out of
    /// step with PersistenceFileName.</summary>
    private string PersistenceFilePath => Path.Combine(FolderPaths.StatePersistence, PersistenceFileName);

    private string _persistenceFileName;

    /// <summary>
    /// The constructor assigns through the property, so its checks run during
    /// construction too: a repository can't be created with a filename the
    /// setter would reject.
    /// </summary>
    public CsvTransactionRepository(string persistenceFileName = "lastUploadedTransactions.csv")
    {
        PersistenceFileName = persistenceFileName;
    }

    /// <summary>The name of the CSV file this repository reads from and writes to.</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when set to an empty string or to a
    /// name that doesn't end in ".csv".</exception>
    public string PersistenceFileName
    {
        get => _persistenceFileName;

        [MemberNotNull(nameof(_persistenceFileName))]
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(PersistenceFileName));
            if (!value.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be a CSV", nameof(PersistenceFileName));
            _persistenceFileName = value;
        }
    }

    /// <inheritdoc/>
    /// <remarks>Checks the data while reading the file: every duplicate
    /// transaction number is found and reported together in one message, so a
    /// damaged file can be fixed in one pass. All named source files must
    /// agree: Save only ever writes one source per file, so two different
    /// names mean the file no longer tells one story. Rows with a blank
    /// source are tolerated: the batch takes its name from the rows that
    /// have one, or records it as "unknown" when none do. A missing or
    /// unreadable number needs no separate check, because reading the field
    /// as an int refuses to parse it, and the wrapper below adds the row
    /// number. A file with headers but no rows loads as null, the same as
    /// no file at all; a completely blank file is treated as damage
    /// instead, because Save always writes headers.</remarks>
    /// <exception cref="InvalidDataException">Thrown when rows carry two
    /// different source file names; when two or more rows share a
    /// transaction number; or when a row cannot be read as transaction data
    /// at all, such as when there is a missing column, a word where a number
    /// belongs, or a malformed line. In that last case the original error
    /// stays attached as the InnerException.</exception>
    public TransactionBatch? Load()
    {
        if (!File.Exists(PersistenceFilePath))
            return null;

        using StreamReader streamReader = new(PersistenceFilePath);
        using CsvReader csv = new(streamReader, CultureInfo.InvariantCulture);

        List<Transaction> loaded = [];
        string batchSourceFile = "";
        int rowNumber = 1;   // the header occupies row 1

        try
        {
            csv.Read();          // step onto the header line
            csv.ReadHeader();    // memorize the column names

            Dictionary<int, List<int>> rowsByTransactionNumber = [];

            while (csv.Read())   // step onto each data row until the file runs out
            {
                rowNumber++;

                string rowSourceFile = csv.GetField<string>(nameof(TransactionBatch.SourceFileName)) ?? "";

                // Blank sources don't vote: a hole in this column will not kill
                // the file. The batch takes its name from the rows that have one;
                // when none do, the TransactionBatch constructor labels it
                // "unknown" at construction.
                if (rowSourceFile.Length > 0 && batchSourceFile.Length == 0)
                    batchSourceFile = rowSourceFile;

                if (rowSourceFile.Length > 0 && !string.Equals(
                    rowSourceFile, batchSourceFile, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(
                        $"Multiple source files detected in {PersistenceFileName}; " +
                        $"review the file to investigate, or delete it to start over.");

                int transactionNumber = csv.GetField<int>(nameof(Transaction.Number));

                if (!rowsByTransactionNumber.ContainsKey(transactionNumber))
                    rowsByTransactionNumber[transactionNumber] = [];

                rowsByTransactionNumber[transactionNumber].Add(rowNumber);

                loaded.Add(new(
                    transactionNumber,
                    csv.GetField<DateOnly>(nameof(Transaction.Date)),
                    csv.GetField<decimal>(nameof(Transaction.Amount)),
                    csv.GetField<string>(nameof(Transaction.Description)) ?? "",
                    csv.GetField<string>(nameof(Transaction.Category)) ?? "Uncategorized"));
            }

            List<string> duplicateReports = [];

            foreach (KeyValuePair<int, List<int>> entry in rowsByTransactionNumber)
            {
                if (entry.Value.Count > 1)
                    duplicateReports.Add(
                        $"multiple entries with transaction number '{entry.Key}' were found at the following rows: {string.Join(", ", entry.Value)}.");
            }

            if (duplicateReports.Count > 0)
                throw new InvalidDataException(
                    $"{PersistenceFileName} has duplicate transaction numbers:\n{string.Join("\n", duplicateReports)}");
        }

        // Catches CsvHelper's own errors (bad value, missing column, malformed
        // line) and the model's validation throws (a negative transaction
        // number), all of which know what went wrong but not where. The
        // InvalidDataExceptions thrown above are neither, so they fly through
        // untouched, as each already tells its own story.
        catch (Exception exception) when (exception is CsvHelperException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{PersistenceFileName} row {rowNumber} could not be read as transaction data.", exception);
        }

        if (loaded.Count == 0) return null;

        TransactionBatch batch = new(loaded, batchSourceFile);
        return batch;
    }

    /// <inheritdoc/>
    /// <remarks>Creates the StatePersistence folder when it doesn't exist yet,
    /// and rewrites the whole file every time: headers first (taken from the
    /// property names via nameof, so they can't drift), then one row per
    /// transaction: dates as yyyy-MM-dd, amounts as plain two-decimal
    /// numbers, and the source file written on every row.</remarks>
    public void Save(TransactionBatch batch)
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);

        using StreamWriter streamWriter = new(PersistenceFilePath);
        using CsvWriter csv = new(streamWriter, CultureInfo.InvariantCulture);

        List<string> headers =
        [
            nameof(Transaction.Number),
            nameof(Transaction.Date),
            nameof(Transaction.Amount),
            nameof(Transaction.Description),
            nameof(Transaction.Category),
            nameof(TransactionBatch.SourceFileName),
        ];

        foreach (string header in headers)
            csv.WriteField(header);
        csv.NextRecord();

        foreach (Transaction transaction in batch.Transactions)
        {
            csv.WriteField(transaction.Number);
            csv.WriteField(transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            csv.WriteField(transaction.Amount.ToString("F2", CultureInfo.InvariantCulture));
            csv.WriteField(transaction.Description);
            csv.WriteField(transaction.Category);
            csv.WriteField(batch.SourceFileName);
            csv.NextRecord();
        }
    }
}