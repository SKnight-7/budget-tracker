# Budget Tracker

A console budget tracker in C# / .NET: load bank transactions from CSV, categorize them automatically by keyword, set budget amounts per category, and see where the money actually went.

**Status: work in progress.** This is a ground-up re-architecture of [budgets-app-modular](https://github.com/SKnight-7/budgets-app-modular), my Python final project for Harvard's CS50P — originally submitted as a single file, later modularized in Python, now being rebuilt in C# with the layered design I didn't yet have the experience to give it the first time.

## Current state

Done and tested:

- **Models** (`Transaction`, `BudgetCategory`) with validation in the property setters, so invalid objects cannot be constructed
- **Categorizer** — keyword matching with a configurable search order to resolve overlaps ("animal hospital" must match Pet Care before Medical ever sees it), covered by an xUnit test suite
- **Budget storage behind the repository pattern** — `IBudgetRepository` is the contract; `CsvBudgetRepository` implements it with CsvHelper, including row-level validation: rows without a category name are rejected, empty keywords are dropped at load (an empty keyword would match every description), duplicate category names are reported in a single exception listing every duplicate and its rows, and CsvHelper's internal errors are rewrapped with the file name and row number attached
- **BudgetManager** — receives its repository through the constructor (dependency injection), typed as the interface, so the storage format can change without touching the manager; provides lookup by menu option number and an update method that persists immediately
- **Console UI toolkit** — table renderer, single- and multi-column menu builders, and a launch greeting (FIGlet banner plus a hand-rolled cowsay, because budgeting is stressful and cows are not)

In progress:

- **Transactions side** — designed, being built next: JSON persistence via `System.Text.Json` behind the same repository pattern (so the finished codebase demonstrates two storage formats built the same way), a separate read-only importer for bank CSV files, and a transaction manager holding both
- **After that** — totals calculation, the budget and transaction views, and the interactive menu loop that ties it all together

Running the app today prints the greeting; the storage layer is built and tested but waits on the interactive loop to be exercised.

## Architecture notes

- Folders sort by role: `Models/`, `Services/` (stateless helpers), `Managers/` (stateful state-holders), `Repositories/`, `Infrastructure/`, `Ui/`, `Defaults/`
- Data folders are split by ownership: `StatePersistence/` holds the app's own files; `BankTransactions/` holds user-provided bank downloads
- Validation follows a layering rule: models guard their invariants and throw on violations; repositories clean or reject external data at the boundary; user-input handling (when the interactive layer lands) re-prompts instead of throwing
- Dependencies are handed in through constructors, never constructed internally

## Built with

- .NET 10 / C#
- [CsvHelper](https://joshclose.github.io/CsvHelper/) — CSV reading and writing
- [Figgle](https://github.com/drewnoakes/figgle) — FIGlet banner
- xUnit — tests

## Build, run, test

```bash
dotnet build
dotnet run --project src/BudgetTracker
dotnet test
```
