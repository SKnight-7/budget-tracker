using BudgetTracker.Ui.Cli;
using BudgetTracker.Repositories;
using BudgetTracker.Managers;
using BudgetTracker.Controllers;

const bool EnableWhimsy = true;

JsonBudgetRepository budgetRepository = new();
JsonTransactionRepository transactionRepository = new();
WellsFargoTransactionSource wellsFargoTransactions = new();

BudgetManager budgetManager = new(budgetRepository);
TransactionManager transactionManager = new(wellsFargoTransactions, transactionRepository);

FinancialController controller = new(budgetManager, transactionManager);

ConsoleDisplay consoleDisplay = new();
ConsoleInput consoleInput = new();

InteractionLoop interactionLoop = new(controller, consoleDisplay, consoleInput, EnableWhimsy);

interactionLoop.Run();