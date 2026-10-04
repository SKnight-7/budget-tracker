using BudgetTracker.Managers;
using BudgetTracker.Services;
using BudgetTracker.Models;

namespace BudgetTracker.Controllers;

/// <summary>
/// The operations layer, where budgets and transactions cross. Each public
/// method is one complete operation composed of manager and service calls:
/// the interaction loop gathers input, calls one method here, and displays
/// whatever comes back. This class holds no domain data (the managers own
/// it), touches no files (the repositories do), and never displays or
/// prompts: no Ui type appears in this file, checkable from the using
/// lines alone, so any front end could drive these same methods.
/// </summary>
public class FinancialController
{
    private readonly BudgetManager _budgetManager;
    private readonly TransactionManager _transactionManager;

    /// <summary>The catalog of actions this application offers, one entry
    /// per operation a front end can start. Front ends read this list and
    /// present the choices their own way; the option numbers identify the
    /// actions when a choice comes back.</summary>
    public List<Option> AppActions { get; } =
    [
        new("Budget Options", "View current budgets", 1),
        new("Budget Options", "Update budgets", 2),
        new("Transaction Options", "Choose a CSV transaction file to load", 3),
        new("Transaction Options", "View transactions by category", 4),
        new("Transaction Options", "View transactions in original order", 5),
        new("Transaction Options", "Recategorize transactions", 6),
    ];

    /// <summary>
    /// Receives the two managers this controller commands and assigns them
    /// to the fields; the constructor does nothing else. Loading saved
    /// data is deliberately a separate method, because loading can throw
    /// on a damaged file, and a method call can be wrapped in a try/catch
    /// block while a constructor that runs during startup cannot be
    /// usefully caught before the rest of the program exists.
    /// </summary>
    public FinancialController(BudgetManager budgetManager, TransactionManager transactionManager)
    {
        _budgetManager = budgetManager;
        _transactionManager = transactionManager;
    }

    /// <summary>Returns the batch of transactions currently in memory, for display.
    /// Never null: TransactionManager keeps a real batch at all times,
    /// empty with source "unknown" before anything loads.</summary>
    public TransactionBatch GetLastUpload() => _transactionManager.LastUpload;

    /// <summary>Builds the financial picture from the current budgets and
    /// the transactions currently in memory. Never null; computed fresh on every
    /// call and stored nowhere.</summary>
    public FinancialSnapshot GetSnapshot() =>
           SnapshotGenerator.GenerateSnapshot(_budgetManager.BudgetCategories, _transactionManager.LastUpload);

    /// <summary>Builds one Option per budget category currently in memory:
    /// the category's general classification as the heading, its name as
    /// the label, and its option number. Built fresh on every call, so the
    /// entries always match the live budget set.</summary>
    public List<Option> GetBudgetOptions()
    {
        return [.. _budgetManager.BudgetCategories.Categories
        .Select(category => new Option(
            category.GeneralClassification,
            category.Name,
            category.OptionNumber
        ))];
    }

    /// <summary>Replaces the in-memory budgets and transactions with
    /// whatever the repositories have stored. When nothing is stored yet,
    /// the budgets side saves its defaults and the transactions side keeps
    /// its empty batch. Anything a repository throws while reading, such
    /// as a damaged file, travels up unchanged.</summary>
    public void LoadSavedData()
    {
        _budgetManager.LoadSavedBudgets();
        _transactionManager.LoadSavedTransactions();
    }

    /// <summary>Finds a transaction in the batch currently in memory.</summary>
    /// <returns>The matching transaction, or null when no transaction in
    /// the current batch has that number.</returns>
    public Transaction? FindTransaction(int transactionNumber) =>
        _transactionManager.FindByTransactionNumber(transactionNumber);

    /// <summary>Finds a budget category by its menu option number. Accepts
    /// null so callers can pass a failed parse straight through.</summary>
    /// <returns>The matching category, or null when the option number is
    /// null or no category has it.</returns>
    public BudgetCategory? FindCategory(int? optionNumber) =>
        _budgetManager.FindByOptionNumber(optionNumber);

    /// <summary>Assigns the given category's name to the given transaction;
    /// the manager persists the whole batch of transactions in the same
    /// call. The transaction must be one of the current batch's own: the
    /// manager checks object identity and refuses any other transaction
    /// with an ArgumentException.</summary>
    public void RecategorizeTransaction(Transaction transaction, BudgetCategory newCategory) =>
        _transactionManager.UpdateCategory(transaction, newCategory.Name);

    /// <summary>Sets the given category's budgeted amount; the manager
    /// persists all budgets in the same call. The category must be one of
    /// the manager's own: the manager checks object identity and refuses
    /// any other category with an ArgumentException. The model's setter
    /// refuses a negative or sub-cent amount with an exception.</summary>
    public void UpdateBudgetAmount(BudgetCategory category, decimal newAmount) =>
        _budgetManager.UpdateAmount(category, newAmount);

    /// <summary>Loads a bank file through the transaction source, assigns a
    /// category to every transaction in the new batch by matching against
    /// the live budget set, and saves the result. The new batch replaces
    /// the one in memory.</summary>
    /// <returns>True when a batch was read, categorized, and saved. False
    /// when no file has that name; nothing in memory changes.</returns>
    /// <remarks>Anything the source or a repository throws, such as a file
    /// that does not match the expected layout, travels up unchanged.</remarks>
    public bool ImportBankFile(string fileName)
    {
        if (!_transactionManager.LoadBankTransactions(fileName))
            return false;

        foreach (Transaction transaction in _transactionManager.LastUpload.Transactions)
            transaction.Category = Categorizer.Categorize(_budgetManager.BudgetCategories, transaction.Description);

        _transactionManager.SaveTransactions();

        return true;
    }

}

