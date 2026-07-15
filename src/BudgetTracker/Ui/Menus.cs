namespace BudgetTracker.Ui;

public static class Menus
{
    private const string StartUnderline = "\x1B[4m";
    private const string ResetStyling = "\x1B[0m";

    public static string Generate(List<MenuOption> options, string title = "MENU")
    {
        List<string> menuToDisplay = [$"\n\n{StartUnderline}{title}{ResetStyling}"];

        foreach (var group in options.OrderBy(o => o.OptionNumber).GroupBy(o => o.GeneralClassification))
        {
            menuToDisplay.Add($"\n{group.Key}:");
            foreach (MenuOption option in group)
            {
                menuToDisplay.Add($"    {option.OptionNumber} - {option.Label}");
            }
        }

        return string.Join("\n", menuToDisplay) + "\n";
    }
}