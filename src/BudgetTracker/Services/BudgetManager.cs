using System.Diagnostics.CodeAnalysis;
using BudgetTracker.Data;
using BudgetTracker.Models;
using CsvHelper;
using System.Globalization;

namespace BudgetTracker.Services;

/// <summary>
/// Holds the live budget state for a session: every category keyed by its name,
/// and the running tallies of money received and spent.
/// </summary>
public class BudgetManager
{
    private string _csvFileName;
    /// <summary>The name of the CSV file this manager's budgets are saved to.</summary>
    /// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
    /// <exception cref="ArgumentException">Thrown when set to an empty string or to a
    /// name that doesn't end in ".csv".</exception>
    public string CsvFileName
    {
        get => _csvFileName;

        [MemberNotNull(nameof(_csvFileName))]
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            if (!value.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be a CSV", nameof(CsvFileName));
            _csvFileName = value;
        }
    }

    /// <summary>Every budget category, fetchable by its name. Each BudgetCategory
    /// carries: GeneralClassification, Name, Keywords, OptionNumber, AmtBudgeted,
    /// and SearchOrder — documented in full on the class itself.</summary>
    public Dictionary<string, BudgetCategory> BudgetCategories { get; set; }

    /// <summary>Running tally of money received, per income category.</summary>
    public Dictionary<string, decimal> IncomeByCategory { get; set; }

    /// <summary>Running tally of money spent, per expense category, including the
    /// "Uncategorized" catch-all.</summary>
    public Dictionary<string, decimal> ExpensesByCategory { get; set; }

    /// <summary>Where the budgets file lives on disk — computed from the storage
    /// root and CsvFileName on every read, so it can never fall out of step.</summary>
    private string FilePath => Path.Combine(StoragePaths.Root, CsvFileName);

    /// <summary>
    /// The constructor assigns through the properties, so their checks run during
    /// construction too. A manager starts from the default categories, keyed by
    /// name, with every tally at zero and "Uncategorized" ready to collect
    /// unmatched spending.
    /// </summary>
    public BudgetManager(string csvFileName = "currentBudgets.csv")
    {
        CsvFileName = csvFileName;
        BudgetCategories = DefaultCategories.GetDefaults().ToDictionary(category => category.Name);

        IncomeByCategory = new();
        ExpensesByCategory = new();
        ExpensesByCategory["Uncategorized"] = 0m;

        foreach (BudgetCategory category in BudgetCategories.Values)
        {
            if (category.GeneralClassification == "Income")
                IncomeByCategory[category.Name] = 0m;
            else
                ExpensesByCategory[category.Name] = 0m;
        }
    }

    public void SaveBudgets()
    {
        Directory.CreateDirectory(StoragePaths.Root);

        using StreamWriter streamWriter = new(FilePath);
        using CsvWriter csv = new(streamWriter, CultureInfo.InvariantCulture);

        List<string> headers =
        [
            nameof(BudgetCategory.GeneralClassification),
            nameof(BudgetCategory.Name),
            nameof(BudgetCategory.Keywords),
            nameof(BudgetCategory.OptionNumber),
            nameof(BudgetCategory.AmtBudgeted),
            nameof(BudgetCategory.SearchOrder),
        ];

        foreach (string header in headers)
            csv.WriteField(header);
        csv.NextRecord();

        foreach (BudgetCategory category in BudgetCategories.Values)
        {
            csv.WriteField(category.GeneralClassification);
            csv.WriteField(category.Name);
            csv.WriteField(string.Join("|", category.Keywords));
            csv.WriteField(category.OptionNumber);
            csv.WriteField(category.AmtBudgeted.ToString("F2", CultureInfo.InvariantCulture));
            csv.WriteField(category.SearchOrder);
            csv.NextRecord();
        }
    }


}