namespace BudgetTracker.Data;

/// <summary>Where the app's data files live on disk.</summary>
public static class StoragePaths
{
    /// <summary>The folder holding all persisted data files, next to the app itself.</summary>
    public static readonly string Root = Path.Combine(AppContext.BaseDirectory, "Storage");
}



