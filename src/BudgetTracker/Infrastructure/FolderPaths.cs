namespace BudgetTracker.Infrastructure;

public static class FolderPaths
{
    private const string BankTransactionsFolderName = "BankTransactions";
    private const string StatePersistenceFolderName = "StatePersistence";

    public static readonly string BankTransactions = FindFolder(BankTransactionsFolderName);
    public static readonly string StatePersistence = FindFolder(StatePersistenceFolderName);

    private static string FindFolder(string folderToFind)
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
                return Path.Combine(dir.FullName, folderToFind);
            dir = dir.Parent;
        }
        return Path.Combine(AppContext.BaseDirectory, folderToFind);
    }
}