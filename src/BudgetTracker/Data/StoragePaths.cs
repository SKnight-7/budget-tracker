namespace BudgetTracker.Data;

/// <summary>Where the app's data files live: the project's own Storage folder,
/// found by walking up from the running app to the nearest .csproj — or, when
/// no project exists above (a published app), a Storage folder beside the app.</summary>
public static class StoragePaths
{
    public static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
                return Path.Combine(dir.FullName, "Storage");
            dir = dir.Parent;
        }
        return Path.Combine(AppContext.BaseDirectory, "Storage");
    }
}



