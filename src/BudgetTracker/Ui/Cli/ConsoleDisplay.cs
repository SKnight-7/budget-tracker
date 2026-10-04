using BudgetTracker.Models;

namespace BudgetTracker.Ui.Cli;

/// <summary>
/// The console implementation of <see cref="IDisplay"/>: formats data as
/// text and writes it to the terminal. Its sibling
/// <see cref="ConsoleInput"/> implements <see cref="IInput"/>; the two are
/// separate classes because the two contracts are separate and the halves
/// share nothing.
/// </summary>
public class ConsoleDisplay : IDisplay
{
    /// <inheritdoc/>
    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc/>
    /// <remarks>Writes to the standard error stream, so when a user
    /// redirects the program's output to a file, errors still appear on
    /// screen.</remarks>
    public void DisplayError(string errorMessage)
    {
        Console.Error.WriteLine(errorMessage);
    }

    /// <inheritdoc/>
    public void DisplayWhimsy(bool enableWhimsy)
    {
        Console.Write(Whimsy.ApplyWhimsy(enableWhimsy));
    }

    /// <inheritdoc/>
    public void DisplayMainMenu(List<Option> options, string menuTitle)
    {
        Console.WriteLine(Menus.Generate(options, menuTitle));
    }

    /// <inheritdoc/>
    /// <remarks>Passes no column count, so Menus.GenerateColumned's default
    /// (four columns) applies; to change the printed column count, pass an
    /// explicit third argument here. IDisplay.DisplayBudgetMenu deliberately
    /// has no column parameter because no front end other than CLI
    /// arranges options into character columns.</remarks>
    public void DisplayBudgetMenu(List<Option> options, string menuTitle)
    {
        Console.WriteLine(Menus.GenerateColumned(options, menuTitle));
    }

    /// <inheritdoc/>
    public void DisplayBudgets(FinancialSnapshot snapshot)
    {
        string displayTitle = "YOUR CURRENT BUDGETS";
        List<string> incomeHeaders = ["Income Category", "Expected", "Received", "Pending"];
        List<string> expenseHeaders = ["Budget Category", "Budgeted", "Expended", "Remaining"];
        List<Alignment> alignments =
            [Alignment.Left, Alignment.Right, Alignment.Right, Alignment.Right];

        List<List<string>> incomeRows = BuildTableRows(snapshot.IncomeRows);
        List<List<string>> expenseRows = BuildTableRows(snapshot.ExpenseRows);

        string header = $"{TextLayout.Underline(displayTitle)}\n"
                      + $"Based on transactions from: {snapshot.SourceFileName}\n"
                      + "\n";

        // Both row lists empty here means every category was skipped as
        // zero-budgeted and zero-actual, or none exist at all. The header
        // still prints, source file included, and only the body is replaced.
        // (The Python original replaced the header too; keeping it is
        // deliberate, because the source file is still true and useful.)
        if (incomeRows.Count == 0 && expenseRows.Count == 0)
        {
            Console.WriteLine(header + "There are no budgets to display.\n");
            return;
        }

        List<string> incomeLabels =
        [
            "Total expected income:",
            "Total received:",
            "Available to allocate:",
        ];

        List<string> incomeAmounts =
        [
            snapshot.TotalExpectedIncome.ToString("C"),
            snapshot.TotalReceived.ToString("C"),
            snapshot.AvailableToAllocate.ToString("C"),
        ];

        List<string> expenseLabels =
        [
            "Unbudgeted:",
            "",
            "Total budgeted:",
            "Total expended:",
            "Unspent balance:",
        ];

        List<string> expenseAmounts =
        [
            snapshot.UnbudgetedExpenses.ToString("C"),
            "",
            snapshot.TotalBudgetedExpenses.ToString("C"),
            snapshot.TotalExpended.ToString("C"),
            snapshot.UnspentBalance.ToString("C"),
        ];

        string incomeSummary = BuildSummary(incomeLabels, incomeAmounts);
        string expenseSummary = BuildSummary(expenseLabels, expenseAmounts);

        string textToDisplay =
                        header
                        + $"{Tables.Generate(incomeHeaders, incomeRows, alignments)}\n"
                        + $"{incomeSummary}\n"
                        + "\n"
                        + $"{Tables.Generate(expenseHeaders, expenseRows, alignments)}\n"
                        + $"{expenseSummary}\n"
                        + "\n";

        Console.WriteLine(textToDisplay);
    }

    /// <inheritdoc/>
    public void DisplayTransactions(TransactionBatch batch)
    {
        string displayTitle = "LAST UPLOADED TRANSACTIONS";
        List<string> headers = ["Transaction #", "Date", "Amount", "Description", "Category"];
        List<Alignment> alignments =
            [Alignment.Right, Alignment.Center, Alignment.Right, Alignment.Left, Alignment.Left];

        List<List<string>> rows = [.. batch.Transactions
            .Select(transaction => new List<string>
            {
                transaction.Number.ToString(),
                transaction.Date.ToString("yyyy-MM-dd"),
                transaction.Amount.ToString("C"),
                transaction.Description,
                transaction.Category,
            })];

        Console.WriteLine($"{TextLayout.Underline(displayTitle)}\n"
                        + $"Transactions from: {batch.SourceFileName}\n"
                        + "\n"
                        + Tables.Generate(headers, rows, alignments));
    }

    /// <summary>Turns category snapshots into table rows for the budgets
    /// display: one row per category, holding its name and its budgeted,
    /// actual, and difference amounts as currency-formatted text. Serves
    /// the income table and the expense table alike, since their cell
    /// contents are identical and only their headers differ.</summary>
    private List<List<string>> BuildTableRows(List<CategorySnapshot> categories)
    {
        return [.. categories
            .Where(category => category.BudgetedAmount !=0 || category.ActualAmount !=0)
            .Select(category => new List<string>
            {
                category.CategoryName,
                category.BudgetedAmount.ToString("C"),
                category.ActualAmount.ToString("C"),
                category.Difference.ToString("C"),
            })];
    }

    /// <summary>Builds the aligned label-and-amount block printed beneath a
    /// table: labels padded left to the longest label, amounts padded right
    /// to the longest amount so the figures stack, three spaces between.
    /// Each line is trimmed of trailing spaces, which turns an empty
    /// label-and-amount pair into a genuinely blank separator line, and the
    /// lines are joined with no trailing newline, like Tables and Menus.</summary>
    /// <exception cref="ArgumentException">Thrown when the two lists differ
    /// in length, since each label pairs with the amount at its index.</exception>
    private string BuildSummary(List<string> labels, List<string> amounts)
    {
        if (labels.Count != amounts.Count)
            throw new ArgumentException($"Labels and amounts must pair up; got {labels.Count} labels and {amounts.Count} amounts.");

        int labelWidth = labels.Max(label => label.Length);
        int amountWidth = amounts.Max(amount => amount.Length);
        int spaceBetween = 3;

        List<string> rows = [];

        for (int i = 0; i < labels.Count; i++)
        {
            rows.Add((TextLayout.PadCell(labels[i], labelWidth, Alignment.Left)
                            + new string(' ', spaceBetween)
                            + TextLayout.PadCell(amounts[i], amountWidth, Alignment.Right))
                            .TrimEnd());
        }

        return string.Join("\n", rows);
    }
}
