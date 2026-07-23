namespace BudgetTracker.Ui;

public static class Tables
{
    private const int CellPadding = 1;

    public static string Generate(List<string> headers, List<List<string>> rows,
                                  List<Alignment>? alignments = null,
                                  bool borders = true,
                                  List<int>? widths = null,
                                  int cellPadding = CellPadding)
    {
        List<string> errorMessages = [];

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].Count != headers.Count)
                errorMessages.Add($"rows[{i}] has {rows[i].Count} cells, expected {headers.Count}.");
        }
        if (alignments is not null && alignments.Count != headers.Count)
            errorMessages.Add($"{alignments.Count} alignments provided; expected {headers.Count} or none.");
        if (widths is not null && widths.Count != headers.Count)
            errorMessages.Add($"{widths.Count} widths provided; expected {headers.Count} or none.");

        if (errorMessages.Count > 0)
            throw new ArgumentException(string.Join("\n", errorMessages));

        alignments ??= [.. Enumerable.Repeat(Alignment.Left, headers.Count)];
        widths ??= ColumnWidths(headers, rows);




    }

    private static List<int> ColumnWidths(List<string> headers, List<List<string>> rows)
    {
        List<int> widths = [.. headers.Select(header => header.Length)];
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < rows[i].Count; j++)
                widths[j] = Math.Max(widths[j], rows[i][j].Length);
        }
        return widths;
    }
}