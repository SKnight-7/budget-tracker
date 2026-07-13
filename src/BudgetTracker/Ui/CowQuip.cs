namespace BudgetTracker.Ui;

/// <summary>Where the cow stands beneath the speech bubble.</summary>
public enum CowPosition
{
    /// <summary>Under the middle of the bubble.</summary>
    Centered,
    /// <summary>Near the bubble's right edge, like the original cowsay.</summary>
    Classic
}

/// <summary>
/// A hand-rolled cowsay for short quips: wraps a phrase in a speech bubble with an
/// ASCII cow beneath it. Phrases are capped at 100 characters and balanced across
/// at most two lines, a design meant to showcase one-liners; longer text would call
/// for paragraph-style wrapping. This cow quips; it does not deliver dissertations.
/// </summary>
public static class CowQuip
{
    private const string Cow = """
        \   ^__^
         \  (oo)\_______
            (__)\       )\/\
                ||----w |
                ||     ||
        """;
    private const string DefaultPhrase = "Just sayin' hay!";
    private const int MaxSingleLine = 50;

    // Searches outward from the phrase's center for the nearest occurrence of any
    // character in the caller-supplied targets, so a two-line split lands as close
    // to balanced as possible. Which characters make good split points is the
    // caller's decision, not this method's.
    private static int FindSplitPoint(string phrase, char[] targets, int window = 10)
    {
        int center = phrase.Length / 2;
        for (int offset = 0; offset <= window; offset++)
        {
            int left = center - offset;
            int right = center + offset;

            if (left >= 0 && targets.Contains(phrase[left])) return left;
            if (right < phrase.Length && targets.Contains(phrase[right])) return right;
        }
        return -1;
    }

    // Long phrases split into two lines, preferring a punctuation break near the
    // center, then a space, then (for unbroken text) a hard split at the midpoint.
    private static string[] WrapPhrase(string phrase)
    {
        if (phrase.Length <= MaxSingleLine)
            return [phrase];

        char[] punctuation = [ '.', ',', '!', '?', ';', ':' ];
        char[] space = [' '];

        int splitAt = FindSplitPoint(phrase, punctuation, 10);
        if (splitAt == -1)
            splitAt = FindSplitPoint(phrase, space, 15);
        if (splitAt == -1)
            splitAt = phrase.Length / 2;

        string line1 = phrase[..(splitAt + 1)].Trim();
        string line2 = phrase[(splitAt + 1)..].Trim();
        return [line1, line2];            
    }

    // Pads each line of the cow so it stands at the chosen position beneath
    // a speech bubble of the given text width.
    private static string MoveCow(int width, CowPosition position = CowPosition.Centered)
    {
        string[] cowLines = Cow.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int bubbleWidth = width + 4;
        int offset = position == CowPosition.Classic ? bubbleWidth - 2 : bubbleWidth / 2;
        string cowPad = new string(' ', offset);
        return string.Join("\n", cowLines.Select(cowLine => cowPad + cowLine));
    }
                    
    /// <summary>Wraps a phrase in a speech bubble with the cow beneath it.</summary>
    /// <param name="phrase">What the cow says. Null or anything over 100 characters
    /// is replaced with the default phrase.</param>
    /// <returns>The bubble and cow as a multi-line string.</returns>
    public static string Say(string phrase = DefaultPhrase)
    {
        if (phrase is null || phrase.Length > 100)
            phrase = DefaultPhrase;

        string[] lines = WrapPhrase(phrase);
        int width = lines.Max(line => line.Length);
               
        string topBorder = " " + new string('_', width + 2);
        string textRows = string.Join("\n", lines.Select(line => $"| {line.PadRight(width)} |"));
        string bottomBorder = " " + new string('=', width + 2);

        return $"{topBorder}\n{textRows}\n{bottomBorder}\n{MoveCow(width)}";
    }
}