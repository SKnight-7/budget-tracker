using BudgetTracker.Ui;
using BudgetTracker.Services;
using BudgetTracker.Data;

Console.WriteLine(Whimsy.ApplyWhimsy());

BudgetManager manager = new();
manager.SaveBudgets();
Console.WriteLine("Saved to: " + StoragePaths.Root);