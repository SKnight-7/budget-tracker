namespace BudgetTracker.Ui;
public enum CowPosition
{
    Centered,
    Classic
}
public static class CowQuip
{
    private const string Cow = @"
\   ^__^
 \  (oo)\_______
    (__)\       )\/\
        ||----w |
        ||     ||";
    private const string DefaultPhrase = "Just sayin' hay!";
    private const int MaxSingleLine = 50;

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

    private static string MoveCow(int width, CowPosition position = CowPosition.Centered)
    {
        string[] cowLines = Cow.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int bubbleWidth = width + 4;
        int offset = position == CowPosition.Classic ? bubbleWidth - 2 : bubbleWidth / 2;
        string cowPad = new string(' ', offset);
        return string.Join("\n", cowLines.Select(cowLine => cowPad + cowLine));
    }
                    
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