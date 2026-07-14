using BudgetTracker.Ui;
using BudgetTracker.Models;

Console.WriteLine(Whimsy.ApplyWhimsy());

Transaction coffee = new(1, new DateOnly(2026, 7, 14), -6.50m, "PEETS COFFEE #123 SACRAMENTO", "Eating Out");
Console.WriteLine(coffee);
