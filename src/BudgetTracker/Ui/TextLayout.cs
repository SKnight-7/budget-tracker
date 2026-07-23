namespace BudgetTracker.Ui;

/// <summary>
/// The shared text-layout toolbox: styling and cell-padding used by every
/// console display builder (Menus, Tables).
/// </summary>
public static class TextLayout
{
    // ANSI escape codes: \x1B is the invisible ESC character, "[4m" switches
    // underlining on, and "[0m" returns all styling to normal.
    public const string StartUnderline = "\x1B[4m";
    public const string ResetStyling = "\x1B[0m";

    /// <summary>Returns the text wrapped in the terminal's underline codes.</summary>
    public static string Underline(string text) => $"{StartUnderline}{text}{ResetStyling}";

    /// <summary>Places text within a width slot, padded per the alignment; centered
    /// text with odd leftover space sits one space left of true center.</summary>
    /// <remarks>Padding is measured from the plain text BEFORE any styling is applied,
    /// so the invisible escape characters never distort the width arithmetic. Text
    /// longer than its slot overflows rather than crashing or truncating.</remarks>
    public static string PadCell(string text, int width, Alignment alignment, bool underline = false)
    {
        int padding = Math.Max(0, width - text.Length);
        string styled = underline ? Underline(text) : text;

        return alignment switch
        {
            Alignment.Right => new string(' ', padding) + styled,
            Alignment.Center => new string(' ', padding / 2) + styled + new string(' ', padding - padding / 2),
            _ => styled + new string(' ', padding),
        };
    }
}
