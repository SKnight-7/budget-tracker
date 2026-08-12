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
    /// <summary>Where the transactions file lives on disk — computed from the state
    /// persistence folder and CsvFileName on every read, so it can never fall
    /// out of step.</summary>
    private string FilePath => Path.Combine(FolderPaths.StatePersistence, CsvFileName);

    private string _csvFileName;

    /// <summary>
    /// The constructor assigns through the property, so its checks run during
    /// construction too: a repository can't be created with a filename the
    /// setter would reject.
    /// </summary>
    public CsvTransactionRepository(string csvFileName = "lastUploadedTransactions.csv")
    {
        CsvFileName = csvFileName;
    }

    /// <summary>The name of the CSV file this repository reads from and writes to.</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when set to an empty string or to a
    /// name that doesn't end in ".csv".</exception>
    public string CsvFileName
    {
        get => _csvFileName;

        [MemberNotNull(nameof(_csvFileName))]
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(CsvFileName));
            if (!value.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be a CSV", nameof(CsvFileName));
            _csvFileName = value;
        }
    }

    /// <inheritdoc/>
    /// <remarks>Checks the data while reading the file: every duplicate
    /// transaction number is found and reported together in one message, so a
    /// damaged file can be fixed in one pass. A missing or unreadable number
    /// needs no separate check — reading the field as an int refuses to parse
    /// it, and the wrapper below adds the row number.</remarks>
    /// <exception cref="InvalidDataException">Thrown when two or more rows
    /// share a transaction number, or when a row cannot be read as transaction
    /// data at all — a missing column, a word where a number belongs, a
    /// malformed line. In that case the original error stays attached as the
    /// InnerException.</exception>
    public List<Transaction>? Load()
    {
        if (!File.Exists(FilePath))
            return null;

        using StreamReader streamReader = new(FilePath);
        using CsvReader csv = new(streamReader, CultureInfo.InvariantCulture);

        List<Transaction> loaded = [];
        int rowNumber = 1;   // the header occupies row 1

        try
        {
            csv.Read();          // step onto the header line
            csv.ReadHeader();    // memorize the column names

            Dictionary<int, List<int>> rowsByTransactionNumber = [];

            while (csv.Read())   // step onto each data row until the file runs out
            {
                rowNumber++;
                int transactionNumber = csv.GetField<int>(nameof(Transaction.Number));

                if (!rowsByTransactionNumber.ContainsKey(transactionNumber))
                    rowsByTransactionNumber[transactionNumber] = [];

                rowsByTransactionNumber[transactionNumber].Add(rowNumber);

                loaded.Add(new(
                    transactionNumber,
                    csv.GetField<DateOnly>(nameof(Transaction.Date)),
                    csv.GetField<decimal>(nameof(Transaction.Amount)),
                    csv.GetField<string>(nameof(Transaction.Description)) ?? "",
                    csv.GetField<string>(nameof(Transaction.Category)) ?? "Uncategorized",
                    csv.GetField<string>(nameof(Transaction.SourceFile)) ?? ""));
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
                    $"{CsvFileName} has duplicate transaction numbers:\n{string.Join("\n", duplicateReports)}");
        }

        // Catches CsvHelper's own errors (bad value, missing column, malformed
        // line) and the model's validation throws (a negative transaction
        // number, a source file that isn't a CSV), all of which know what went
        // wrong but not where. The duplicate-number InvalidDataException above
        // is neither, so it flies through untouched — it already carries its
        // row numbers.
        catch (Exception exception) when (exception is CsvHelperException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{CsvFileName} row {rowNumber} could not be read as transaction data.", exception);
        }

        return loaded;
    }

    /// <inheritdoc/>
    /// <remarks>Creates the StatePersistence folder when it doesn't exist yet,
    /// and rewrites the whole file every time: headers first (taken from the
    /// property names via nameof, so they can't drift), then one row per
    /// transaction — dates as yyyy-MM-dd, amounts as plain two-decimal
    /// numbers, and the source file written on every row.</remarks>
    public void Save(List<Transaction> transactions)
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);

        using StreamWriter streamWriter = new(FilePath);
        using CsvWriter csv = new(streamWriter, CultureInfo.InvariantCulture);

        List<string> headers =
        [
            nameof(Transaction.Number),
            nameof(Transaction.Date),
            nameof(Transaction.Amount),
            nameof(Transaction.Description),
            nameof(Transaction.Category),
            nameof(Transaction.SourceFile),
        ];

        foreach (string header in headers)
            csv.WriteField(header);
        csv.NextRecord();

        foreach (Transaction transaction in transactions)
        {
            csv.WriteField(transaction.Number);
            csv.WriteField(transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            csv.WriteField(transaction.Amount.ToString("F2", CultureInfo.InvariantCulture));
            csv.WriteField(transaction.Description);
            csv.WriteField(transaction.Category);
            csv.WriteField(transaction.SourceFile);
            csv.NextRecord();
        }
    }
}