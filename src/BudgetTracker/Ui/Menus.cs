namespace BudgetTracker.Ui;

/// <summary>
/// Builds menu displays from MenuOption entries.
/// </summary>
public static class Menus
{
    // The two dials of the columned layout: every column is the same fixed width,
    // and names wrap inside it, so no single long entry can stretch a column.
    private const int CardWidth = 22;         // fixed content width, "NN: " prefix included
    private const int ColumnGutter = 5;       // spaces between columns
    private const int OptionPrefixWidth = 4;  // "NN: " — number right-aligned to 2, colon, space

    /// <summary>Builds a menu display: an underlined title, then options grouped by
    /// general classification and ordered by option number.</summary>
    /// <param name="options">The menu entries to display, in any order.</param>
    /// <param name="title">The heading shown underlined above the menu.</param>
    /// <returns>The formatted menu as a single string, ready to display.</returns>
    /// <remarks>Groups appear in order of their lowest option number.</remarks>
    public static string Generate(List<MenuOption> options, string title = "MENU")
    {
        List<string> menuToDisplay = [$"\n\n{TextLayout.Underline(title)}"];

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

    /// <summary>Builds a multi-column menu display: an underlined title, then one
    /// card per general classification, arranged left to right in rows.</summary>
    /// <param name="options">The menu entries to display, in any order.</param>
    /// <param name="title">The heading shown underlined above the menu.</param>
    /// <param name="columnCount">How many cards stand side by side per row.</param>
    /// <returns>The formatted menu as a single string, ready to display.</returns>
    /// <remarks>Cards appear in order of their lowest option number. Every column is
    /// the same fixed width (CardWidth) with a fixed gutter between columns, and
    /// option names wrap inside it onto indented continuation lines.</remarks>
    public static string GenerateColumned(List<MenuOption> options, string title = "MENU", int columnCount = 4)
    {
        List<string> menuToDisplay = [$"\n\n{TextLayout.Underline(title)}\n"];

        List<List<string>> cards = [.. options
            .OrderBy(o => o.OptionNumber)
            .GroupBy(o => o.GeneralClassification)
            .Select(BuildCard)];

        foreach (var chunk in cards.Chunk(columnCount))
        {
            menuToDisplay.AddRange(StitchRow([.. chunk]));
            menuToDisplay.Add("");
        }

        return string.Join("\n", menuToDisplay);
    }

    // Greedy word wrap: each line takes on words until the next word won't fit.
    private static List<string> WrapWords(string text, int width)
    {
        if (text.Length <= width)
            return [text];

        List<string> lines = [];
        string currentLine = "";

        foreach (string word in text.Split(' '))
        {
            if (currentLine.Length == 0)
                currentLine = word;
            else if (currentLine.Length + 1 + word.Length <= width)
                currentLine += " " + word;
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }
        lines.Add(currentLine);
        return lines;
    }

    // One classification's entries as display lines: uppercased header first, then
    // each option with its number; wrapped names continue on indented lines.
    private static List<string> BuildCard(IGrouping<string, MenuOption> group)
    {
        List<string> card = [group.Key.ToUpper()];

        foreach (MenuOption option in group)
        {
            List<string> nameLines = WrapWords(option.Label, CardWidth - OptionPrefixWidth);
            card.Add($"{option.OptionNumber,2}: {nameLines[0]}");
            foreach (string continuation in nameLines.Skip(1))
                card.Add($"    {continuation}");
        }
        return card;
    }

    // Reads across the cards: line i of every card side by side, each padded to the
    // fixed column width; cards shorter than the row's tallest contribute blank slots.
    private static List<string> StitchRow(List<List<string>> cards)
    {
        List<string> stitched = [];
        int height = cards.Max(card => card.Count);

        for (int i = 0; i < height; i++)
        {
            string rowLine = "";
            for (int j = 0; j < cards.Count; j++)
            {
                string line = i < cards[j].Count ? cards[j][i] : "";
                rowLine += TextLayout.PadCell(line, CardWidth + ColumnGutter, Alignment.Left, underline: i == 0);
            }
            stitched.Add(rowLine.TrimEnd());
        }
        return stitched;
    }
}