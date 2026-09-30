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
  so an invalid object cannot be constructed. String properties follow one rule:
  when a missing value has a sensible meaning, the setter substitutes it (a blank
  transaction category becomes "Unbudgeted", a blank source name becomes
  "unknown"); when it has none, the setter throws (a budget category must have a
  name). Strings that code compares against ("Income", "Unbudgeted") are declared
  once, as public constants, and referenced everywhere else.
  Above the two base models sit two aggregate models, `BudgetSet` and
  `TransactionBatch`, twins by design: each is the single value its repositories
  trade in, and each does all of its validation in the constructor. Construction is
  the only door, so a set with duplicate category names or a batch with duplicate
  transaction numbers can never exist; duplicates are reported in one exception
  naming every offender. Both aggregate models are covered by the test suite.
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
  mistaken for an empty one and overwritten. Both JSON repositories are covered by
  the test suite, each pointed at its own test file so the app's real data is never
  touched.
- **BudgetManager.** Receives its repository through the constructor (dependency
  injection), typed as the interface, so the storage format can change without
  touching the manager. It trades in `BudgetSet`, with lookup by menu option
  number and an update method that persists immediately.
- **Bank-file import.** `IBankTransactionSource` defines a read-only contract for
  how data enters the app, kept deliberately separate from the app's own
  persistence. `WellsFargoTransactionSource` reads the bank's headerless CSV
  export, assigns transaction numbers from row order, folds check numbers into
  descriptions, and sorts failures by extent: when no row parses, the file is
  reported as the wrong layout; when only some rows fail, every bad row is named in
  one exception; and an empty file is a valid empty batch.
- **TransactionManager.** The transactions twin of BudgetManager, receiving both
  its repository and its bank-file source through the constructor, each typed as
  an interface. It keeps the most recently uploaded batch, finds transactions by
  number, and persists every recategorization in the same call that makes it, so
  no change can exist unsaved.
- **Totals pipeline.** `TransactionGrouper` groups a batch's transactions by
  category name, case-insensitively, into a dictionary of lists. For each budget
  category, `SnapshotGenerator` looks up that category's transactions, totals
  their amounts, and flips the sign of every non-income total so spending displays
  as a positive number; transactions whose category matches no tracked budget are
  combined into a single "Unbudgeted" entry in the same list. Each budget's
  results travel as a `CategorySnapshot`: general classification, category name,
  budgeted amount, actual amount, and a difference computed in the constructor
  from the values passed in. `FinancialSnapshot` receives the finished list and
  computes six overview figures from the income and expense entries; the
  Unbudgeted entry is excluded from all six, so the overview compares the plan
  only against money the plan covers. No repository reads or writes these types,
  and no class keeps an instance: each snapshot is built when requested and handed
  to the caller.
- **Parser.** Wraps the standard TryParse patterns to return null on failure instead
  of throwing, so the interactive layer can validate raw user input with a simple
  null check. Date parsing takes a caller-supplied format so each data source can
  declare its own shape, and uses the invariant culture so stored data parses
  identically on any machine.
- **Console UI toolkit.** Table renderer, single- and multi-column menu builders,
  text layout and alignment helpers, and a launch greeting (FIGlet banner plus a
  hand-rolled cowsay, because budgeting is stressful and cows are not).
- **Two front-end interfaces, split by capability.** `IDisplay` holds the display
  methods; every one receives data, never pre-formatted text, so each implementing
  class does its own formatting and a web front end could implement it. `IInput`
  holds the prompt methods, which only front ends that can ask and wait can
  implement; a web server cannot, which is why the two interfaces are separate.
  `ConsoleUi` implements both, with every display built: the budgets view (two
  tables with aligned summary blocks beneath them, the unbudgeted amount beside
  the expense totals), the transactions view, and the two menus.
- **FinancialController.** The operations layer, where budgets and transactions
  cross. It receives both managers through its constructor, exposes finders that
  return the matching object or null so the interaction loop can validate each
  user entry as it is typed, and composes the import flow: load a bank file,
  categorize every transaction against the live budget set, save. No Ui type
  appears in the file, so any front end could drive the same operations.

In progress:

- **The interaction loop.** The menu conversation that ties input, controller,
  and displays together, and the Program.cs wiring where the concrete
  implementations are chosen and handed in.

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
