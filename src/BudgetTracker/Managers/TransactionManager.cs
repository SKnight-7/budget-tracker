using BudgetTracker.Models;
using BudgetTracker.Repositories;

namespace BudgetTracker.Managers;

/// <summary>
/// Keeps the batch of bank transactions the app is currently working with:
/// the most recently uploaded batch, held in memory for the session.
/// </summary>
public class TransactionManager
{
    private readonly ITransactionRepository _repository;
    private readonly IBankTransactionSource _source;

    /// <summary>The most recently uploaded batch: the transactions and the
    /// name of the bank file they came from. On a fresh start, before any
    /// upload exists, the batch is empty and its source reads "unknown".</summary>
    public TransactionBatch LastUpload { get; private set; }

    /// <summary>
    /// The manager receives both of its tools through the constructor: the
    /// bank-file reader can be any class implementing IBankTransactionSource,
    /// and the storage can be any class implementing ITransactionRepository.
    /// The manager only ever calls the interfaces' methods, so no bank-format
    /// or storage-format code appears anywhere in this class. It starts from
    /// an empty batch; the empty string is deliberate, because
    /// TransactionBatch's own constructor turns a blank source name into
    /// "unknown".
    /// </summary>
    public TransactionManager(IBankTransactionSource source, ITransactionRepository repository)
    {
        _source = source;
        _repository = repository;

        LastUpload = new([], "");
    }

    /// <summary>Hands the batch currently in memory to the repository for storage.</summary>
    public void SaveTransactions() =>
        _repository.Save(LastUpload);

    /// <summary>
    /// Replaces the in-memory batch with whatever the repository has stored:
    /// the stored batch when there is one, or a fresh empty batch when there
    /// is not. After this method runs, memory always equals the file's truth,
    /// whenever and however often it is called. Unlike the budgets side, the
    /// nothing-stored case seeds nothing, because there are no default
    /// transactions worth writing.
    /// </summary>
    /// <remarks>Anything the repository throws while reading, such as a rejected
    /// row or damaged data, travels up through this method unchanged.</remarks>
    public void LoadSavedTransactions()
    {
        TransactionBatch? loaded = _repository.Load();
        LastUpload = loaded ?? new([], "");
    }

    /// <summary>Replaces the in-memory batch with the contents of a bank
    /// file, read through the bank source. The new batch arrives with every
    /// transaction Unbudgeted; categorizing it is the controller's job,
    /// done in place through LastUpload.</summary>
    /// <param name="fileName">The name of the bank file to read.</param>
    /// <returns>True when a batch was read and kept. False when no file has
    /// that name: the batch in memory does not change, and what the miss
    /// means is the caller's decision (typically: re-prompt).</returns>
    /// <remarks>Anything the source throws while reading, such as a file
    /// that does not match the expected layout or damaged rows, travels up
    /// through this method unchanged.</remarks>
    public bool LoadBankTransactions(string fileName)
    {
        TransactionBatch? loaded = _source.Load(fileName);

        if (loaded is null)
            return false;

        LastUpload = loaded;
        return true;
    }

    /// <summary>Finds the transaction a user selected by transaction number.
    /// Accepts null so callers can pass a failed parse straight through.</summary>
    /// <returns>The matching transaction, or null when the number is null or
    /// no transaction in the current batch has it; the caller decides what a
    /// miss means (typically: re-prompt).</returns>
    public Transaction? FindByTransactionNumber(int? transactionNumber) =>
        LastUpload.Transactions.FirstOrDefault(transaction => transaction.Number == transactionNumber);

    /// <summary>Sets a transaction's category and persists the whole batch in
    /// one call, so no recategorization can ever exist unsaved.</summary>
    /// <param name="transaction">The transaction to update. Must be one of
    /// this batch's own transactions, typically obtained from
    /// FindByTransactionNumber; the guard checks object identity, not number.</param>
    /// <param name="newCategory">The new category name. The manager does not
    /// check the name against the budget categories: budgets and transactions
    /// cross only in the controller, so vouching for the name is the
    /// caller's job.</param>
    /// <exception cref="ArgumentException">Thrown when the given transaction
    /// is not in this batch.</exception>
    public void UpdateCategory(Transaction transaction, string newCategory)
    {
        if (!LastUpload.Transactions.Contains(transaction))
            throw new ArgumentException($"The given transaction ('Number {transaction.Number}') is not in this batch.", nameof(transaction));

        transaction.Category = newCategory;
        SaveTransactions();
    }
}