namespace BudgetTracker.Ui;

/// <summary>
/// Builds menu displays from MenuOption entries.
/// </summary>
public static class Menus
{
    // ANSI escape codes: \x1B is the invisible ESC character, "[4m" switches
    // underlining on, and "[0m" returns all styling to normal.
    private const string StartUnderline = "\x1B[4m";
    private const string ResetStyling = "\x1B[0m";

    /// <summary>Builds a menu display: an underlined title, then options grouped by
    /// general classification and ordered by option number.</summary>
    /// <param name="options">The menu entries to display, in any order.</param>
    /// <param name="title">The heading shown underlined above the menu.</param>
    /// <returns>The formatted menu as a single string, ready to display.</returns>
    /// <remarks>Groups appear in order of their lowest option number.</remarks>
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