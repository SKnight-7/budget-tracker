namespace BudgetTracker.Models;

/// <summary>One entry offered for selection: the heading it is grouped
/// under, the label naming it, and the number that identifies it when the
/// user's choice comes back. Front ends express a list of options in
/// their own way; a console shows a numbered menu and reads the number
/// back, while a web page could render the same options as buttons or
/// links. The record describes the choice, not its presentation.</summary>
public record Option(string Heading, string Label, int OptionNumber);
