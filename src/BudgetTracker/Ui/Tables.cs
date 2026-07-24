namespace BudgetTracker.Ui;

/// <summary>
/// Draws grid tables from headers and rows of text — this app's homegrown
/// replacement for the Python original's tabulate dependency.
/// </summary>
public static class Tables
{
    private const int CellPadding = 1;

    /// <summary>Builds a bordered grid table: headers demarcated by an '=' line,
    /// every cell padded and aligned per its column.</summary>
    /// <param name="headers">One column heading per column.</param>
    /// <param name="rows">The table body: each row a list of cell text, one per column.</param>
    /// <param name="alignments">How each column's text sits; all Left when omitted.</param>
    /// <param name="cellPadding">Spaces inside each cell, either side of the content.</param>
    /// <returns>The finished table as a single multi-line string.</returns>
    /// <exception cref="ArgumentException">Thrown when any row's cell count doesn't match
    /// the headers, or a supplied alignments list is the wrong length — one exception
    /// gathering every problem found.</exception>
    /// <remarks>Cells are text only: callers format their own numbers, dates, and money.
    /// Columns are always sized to their content, so cells never overflow or wrap.</remarks>
    public static string Generate(List<string> headers, List<List<string>> rows,
                                  List<Alignment>? alignments = null,
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

        if (errorMessages.Count > 0)
            throw new ArgumentException(string.Join("\n", errorMessages));

        alignments ??= [.. Enumerable.Repeat(Alignment.Left, headers.Count)];
        List<int> widths = ColumnWidths(headers, rows);

        List<string> paddedHeaders = [];
        for (int j = 0; j < headers.Count; j++)
            paddedHeaders.Add(TextLayout.PadCell(headers[j], widths[j], alignments[j]));

        List<List<string>> paddedRows = [];
        for (int i = 0; i < rows.Count; i++)
        {
            List<string> paddedRow = [];
            for (int j = 0; j < rows[i].Count; j++)
                paddedRow.Add(TextLayout.PadCell(rows[i][j], widths[j], alignments[j]));
            paddedRows.Add(paddedRow);
        }

        List<string> finalTable = [];
        finalTable.Add(DrawBorder(widths, cellPadding));
        finalTable.Add(DrawContent(paddedHeaders, cellPadding));
        finalTable.Add(DrawBorder(widths, cellPadding, '='));

        for (int i = 0; i < paddedRows.Count; i++)
        {
            finalTable.Add(DrawContent(paddedRows[i], cellPadding));
            finalTable.Add(DrawBorder(widths, cellPadding));
        }

        return string.Join("\n", finalTable);
    }

    // Each column ends up as wide as its widest occupant — its header, or any
    // row's cell at that position.
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

    // One horizontal line: a separator at every column edge, fill characters
    // between ('=' fill draws the header demarcation).
    private static string DrawBorder(List<int> widths, int cellPadding, char fill = '-', string separator = "+")
    {
        string border = separator;
        for (int i = 0; i < widths.Count; i++)
            border += new string(fill, widths[i] + (cellPadding * 2)) + separator;
        return border;
    }

    // One row of cells, dressed in cell padding and separators. Cells arrive
    // pre-padded and pre-aligned; this method measures nothing.
    private static string DrawContent(List<string> paddedCells, int cellPadding, string separator = "|")
    {
        string contentRow = separator;
        string padding = new string(' ', cellPadding);

        for (int i = 0; i < paddedCells.Count; i++)
            contentRow += padding + paddedCells[i] + padding + separator;
        return contentRow;
    }
}