using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using BudgetTracker.Services;

namespace BudgetTracker.Repositories;

/// <summary>
/// The Wells Fargo implementation of <see cref="IBankTransactionSource"/>:
/// reads the bank's headerless CSV export from the BankTransactions folder
/// and turns it into a batch of transactions.
/// </summary>
public class WellsFargoTransactionSource : IBankTransactionSource
{
    /// <summary>Reader settings for Wells Fargo's export layout, built once
    /// and reused: the files have no header row, so fields are read by
    /// position instead of by column name.</summary>
    private static readonly CsvConfiguration WellsFargoFormat = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false,
        MissingFieldFound = null,
    };

    /// <inheritdoc/>
    /// <remarks>Reads the file one row at a time, assigning transaction
    /// numbers from row order as it goes: the number is a pointer back into
    /// the bank's own file. Each row's date and amount are probed with the
    /// Parser's null-returning methods rather than trusted; rows that fail
    /// (an unparseable date or amount, or an amount finer than whole cents)
    /// are collected and judged together after the loop. When every row
    /// failed, the file is reported as not matching the expected layout;
    /// when only some failed, the file is reported as damaged, naming every
    /// bad row; when none failed, the batch is returned, empty included (a
    /// quiet date range is a valid truth, and what to tell the user is the
    /// caller's decision). A check number, when present, is folded into the
    /// description as "CHECK number", so check transactions keep their one
    /// identifying mark; a missing description or check field is tolerated
    /// as blank.</remarks>
    /// <exception cref="InvalidDataException">Thrown when the file does not
    /// match the expected Wells Fargo layout; when readable rows sit next to
    /// damaged ones; or when the file cannot be read as CSV at all. In that
    /// last case the original error stays attached as the InnerException.</exception>
    public TransactionBatch? Load(string fileName)
    {
        string sourceFilePath = Path.Combine(FolderPaths.BankTransactions, fileName);

        if (!File.Exists(sourceFilePath))
            return null;

        using StreamReader streamReader = new(sourceFilePath);
        using CsvReader csv = new(streamReader, WellsFargoFormat);

        List<Transaction> loaded = [];
        int rowNumber = 0; // there are no headers, so we'll be stepping straight into the data

        try
        {
            List<int> badRows = [];

            while (csv.Read()) // step onto each data row until the file runs out
            {
                rowNumber++;

                DateOnly? date = Parser.ParseDate(csv.GetField(0), "M/d/yyyy");
                if (date is null)
                {
                    badRows.Add(rowNumber);
                    continue;
                }

                decimal? amount = Parser.ParseDecimal(csv.GetField(1));
                if (amount is null || (amount.Value != Math.Round(amount.Value, 2)))
                {
                    badRows.Add(rowNumber);
                    continue;
                }

                string checkNum = csv.GetField(3) ?? "";
                string description = csv.GetField(4) ?? "";

                if (checkNum != "")
                    checkNum = description == "" ? $"CHECK {checkNum}" : $"CHECK {checkNum}: ";

                loaded.Add(new(
                    rowNumber,
                    date.Value,
                    amount.Value,
                    $"{checkNum}{description}"));
            }

            // When no row could be read at all, the file is probably not a
            // Wells Fargo export in the first place, so describe the
            // expected layout rather than pointing at individual rows.
            if (badRows.Count > 0 && loaded.Count == 0)
                throw new InvalidDataException(
                    $"{fileName} could not be read as a Wells Fargo export. Expected five " +
                    "unlabeled columns: date, amount, a placeholder, a check number or blank, " +
                    "and a description.");

            // Now we know there are both bad rows and good ones: assume the format
            // is right, but some rows carry damaged data.
            if (badRows.Count > 0)
                throw new InvalidDataException(
                    $"{fileName} rows {string.Join(", ", badRows)} could not be read as " +
                    "transaction data. Review the file to investigate, or download a fresh copy.");

            return new TransactionBatch(loaded, fileName);
        }

        // With every field fetched as a string and missing positions
        // configured silent, the one thing left that can throw here is
        // CsvHelper itself, failing to read a torn or malformed line; the
        // ArgumentException arm is a safety net for any model refusal the
        // checkpoints above did not predict. The InvalidDataExceptions
        // thrown above are neither, so they fly through untouched, as each
        // already tells its own story. The torn line is the one the reader
        // was stepping onto, one past the last row it finished counting.
        catch (Exception exception) when (exception is CsvHelperException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{fileName} row {rowNumber + 1} could not be read as transaction data.", exception);
        }
    }
}
