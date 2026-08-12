using System.Diagnostics.CodeAnalysis;
using BudgetTracker.Infrastructure;
using BudgetTracker.Models;
using CsvHelper;
using System.Globalization;

namespace BudgetTracker.Repositories;

/// <summary>
/// The CSV implementation of <see cref="IBudgetRepository"/>: keeps the budget
/// categories in a CSV file inside the StatePersistence folder.
/// </summary>
public class CsvBudgetRepository : IBudgetRepository
{
    /// <summary>Where the budgets file lives on disk — computed from the state
    /// persistence folder and CsvFileName on every read, so it can never fall
    /// out of step.</summary>
    private string FilePath => Path.Combine(FolderPaths.StatePersistence, CsvFileName);

    private string _csvFileName;

    /// <summary>
    /// The constructor assigns through the property, so its checks run during
    /// construction too: a repository can't be created with a filename the
    /// setter would reject.
    /// </summary>
    public CsvBudgetRepository(string csvFileName = "currentBudgets.csv")
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
    /// <remarks>Checks the data while reading the file: a row with no category
    /// name is rejected, and empty keyword entries (produced when the keywords
    /// cell is blank or has stray '|' separators) are dropped rather than
    /// loaded, because an empty keyword would match every description.</remarks>
    /// <exception cref="InvalidDataException">Thrown when a row has no category
    /// name; when two or more rows share a category name (compared ignoring
    /// case — the full census of duplicates and their rows is reported in one
    /// message); or when a row cannot be read as budget data at all — a missing
    /// column, a word where a number belongs, a malformed line. In that last
    /// case the original error stays attached as the InnerException.</exception>
    public List<BudgetCategory>? Load()
    {
        if (!File.Exists(FilePath))
            return null;

        using StreamReader streamReader = new(FilePath);
        using CsvReader csv = new(streamReader, CultureInfo.InvariantCulture);

        List<BudgetCategory> loaded = [];
        int rowNumber = 1;   // the header occupies row 1

        try
        {
            csv.Read();          // step onto the header line
            csv.ReadHeader();    // memorize the column names

            Dictionary<string, List<int>> rowsByName = new(StringComparer.OrdinalIgnoreCase);

            while (csv.Read())   // step onto each data row until the file runs out
            {
                rowNumber++;
                string name = csv.GetField<string>(nameof(BudgetCategory.Name)) ?? "";
                if (name.Length == 0)
                    throw new InvalidDataException($"{CsvFileName} row {rowNumber} has no category name.");

                if (!rowsByName.ContainsKey(name))
                    rowsByName[name] = [];

                rowsByName[name].Add(rowNumber);

                loaded.Add(new(
                    csv.GetField<string>(nameof(BudgetCategory.GeneralClassification)) ?? "",
                    name,
                    [.. (csv.GetField<string>(nameof(BudgetCategory.Keywords)) ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries)],
                    csv.GetField<int>(nameof(BudgetCategory.OptionNumber)),
                    csv.GetField<decimal>(nameof(BudgetCategory.AmountBudgeted)),
                    csv.GetField<decimal>(nameof(BudgetCategory.SearchOrder))));
            }

            List<string> duplicateReports = [];

            foreach (KeyValuePair<string, List<int>> entry in rowsByName)
            {
                if (entry.Value.Count > 1)
                    duplicateReports.Add(
                        $"multiple versions of '{entry.Key.ToLower()}' were found at the following rows: {string.Join(", ", entry.Value)}.");
            }

            if (duplicateReports.Count > 0)
                throw new InvalidDataException(
                    $"{CsvFileName} has duplicate category names:\n{string.Join("\n", duplicateReports)}");
        }

        // Catches CsvHelper's own errors (bad value, missing column, malformed
        // line) and the model's validation throws (a negative amount, a zero
        // option number), all of which know what went wrong but not where.
        // The nameless-row InvalidDataException above is neither, so it flies
        // through untouched — it already carries its row number.
        catch (Exception exception) when (exception is CsvHelperException or ArgumentException)
        {
            throw new InvalidDataException(
                $"{CsvFileName} row {rowNumber} could not be read as budget data.", exception);
        }

        return loaded;
    }

    /// <inheritdoc/>
    /// <remarks>Creates the StatePersistence folder when it doesn't exist yet,
    /// and rewrites the whole file every time: headers first (taken from the
    /// property names via nameof, so they can't drift), then one row per
    /// category — amounts as plain two-decimal numbers, keywords joined
    /// by '|'.</remarks>
    public void Save(List<BudgetCategory> categories)
    {
        Directory.CreateDirectory(FolderPaths.StatePersistence);

        using StreamWriter streamWriter = new(FilePath);
        using CsvWriter csv = new(streamWriter, CultureInfo.InvariantCulture);

        List<string> headers =
        [
            nameof(BudgetCategory.GeneralClassification),
            nameof(BudgetCategory.Name),
            nameof(BudgetCategory.Keywords),
            nameof(BudgetCategory.OptionNumber),
            nameof(BudgetCategory.AmountBudgeted),
            nameof(BudgetCategory.SearchOrder),
        ];

        foreach (string header in headers)
            csv.WriteField(header);
        csv.NextRecord();

        foreach (BudgetCategory category in categories)
        {
            csv.WriteField(category.GeneralClassification);
            csv.WriteField(category.Name);
            csv.WriteField(string.Join("|", category.Keywords));
            csv.WriteField(category.OptionNumber);
            csv.WriteField(category.AmountBudgeted.ToString("F2", CultureInfo.InvariantCulture));
            csv.WriteField(category.SearchOrder);
            csv.NextRecord();
        }
    }
}