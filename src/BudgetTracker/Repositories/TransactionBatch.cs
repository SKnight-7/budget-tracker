using BudgetTracker.Models;

namespace BudgetTracker.Repositories;

/// <summary>
/// One saved set of transactions together with the name of the bank file
/// they came from. This is the single value the transaction repositories
/// trade in: Load returns one, Save receives one.
/// </summary>
public record TransactionBatch(List<Transaction> Transactions, string SourceFileName);