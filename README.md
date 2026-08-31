# Budget Tracker

A console budget tracker in C# / .NET: load bank transactions from CSV, categorize
them automatically by keyword, set budget amounts per category, and see where the
money actually went.

**Status: work in progress.** This is a ground-up re-architecture of
[budgets-app-modular](https://github.com/SKnight-7/budgets-app-modular), my Python
final project for Harvard's CS50P. It was originally submitted as a single file,
later modularized in Python, and is now being rebuilt in C# with the layered design
I didn't yet have the experience to give it the first time.

## Current state

Built:

- **Models.** `Transaction` and `BudgetCategory` validate in their property setters,
  so an invalid object cannot be constructed. Above them sit two aggregate models,
  `TrackedBudgets` and `TransactionBatch`, twins by design: each is the single value
  its repositories trade in, and each does all of its validation in the constructor.
  Construction is the only door, so a set with duplicate category names or a batch
  with duplicate transaction numbers can never exist; duplicates are reported in one
  exception naming every offender.
- **Categorizer.** Keyword matching with a configurable search order to resolve
  overlaps ("animal hospital" must match Pet Care before Medical ever sees it),
  covered by an xUnit test suite.
- **Storage, symmetric by contract.** `IBudgetRepository` and
  `ITransactionRepository` define the contracts; each has a CSV implementation
  (CsvHelper) and a JSON implementation (`System.Text.Json`), four repositories built
  the same way. The CSV side validates row by row: rows without a category name are
  rejected, empty keywords are dropped at load (an empty keyword would match every
  description), and CsvHelper's internal errors are rewrapped with file name and row
  number attached. The JSON side distinguishes tolerance from damage: a missing or
  blank file loads as nothing-stored so defaults apply, but text that cannot be
  parsed stops the load with an exception rather than letting a damaged file be
  mistaken for an empty one and overwritten.
- **BudgetManager.** Receives its repository through the constructor (dependency
  injection), typed as the interface, so the storage format can change without
  touching the manager. It trades in `TrackedBudgets`, with lookup by menu option
  number and an update method that persists immediately.
- **Parser.** Wraps the standard TryParse patterns to return null on failure instead
  of throwing, so the interactive layer can validate raw user input with a simple
  null check. Date parsing takes a caller-supplied format so each data source can
  declare its own shape, and uses the invariant culture so stored data parses
  identically on any machine.
- **Console UI toolkit.** Table renderer, single- and multi-column menu builders,
  text layout and alignment helpers, an `IUserInterface` abstraction, and a launch
  greeting (FIGlet banner plus a hand-rolled cowsay, because budgeting is stressful
  and cows are not).

In progress:

- **Bank-file import.** A read-only source, interface plus CSV implementation, that
  reads a bank's downloaded CSV and turns it into transactions. This is how data
  enters the app, kept deliberately separate from the app's own persistence.
- **TransactionManager.** The transactions twin of BudgetManager, holding the live
  transaction state for a session and born holding both the repository and the
  bank-file source.
- **After that.** Totals calculation, the budget and transaction views, and the
  interactive menu loop that ties it all together.

Running the app today prints the greeting; the storage layer waits on the
interactive loop to be exercised.

## Architecture notes

- Folders sort by role: `Models/`, `Services/` (stateless helpers), `Managers/`
  (stateful state-holders), `Repositories/`, `Infrastructure/`, `Ui/`, `Defaults/`
- Data folders are split by ownership: `StatePersistence/` holds the app's own
  files; `BankTransactions/` holds user-provided bank downloads
- Validation follows a layering rule: models guard their invariants and throw on
  violations; repositories clean or reject external data at the boundary; user-input
  handling (when the interactive layer lands) re-prompts instead of throwing, with
  `Parser` as its foundation
- Dependencies are handed in through constructors, never constructed internally

## Built with

- .NET 10 / C#
- [CsvHelper](https://joshclose.github.io/CsvHelper/) for CSV reading and writing
- `System.Text.Json` for JSON persistence (built into .NET)
- [Figgle](https://github.com/drewnoakes/figgle) for the FIGlet banner
- xUnit for tests

## Build, run, test

```bash
dotnet build
dotnet run --project src/BudgetTracker
dotnet test
```
