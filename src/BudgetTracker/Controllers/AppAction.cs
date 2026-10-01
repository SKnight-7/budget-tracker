namespace BudgetTracker.Controllers;

/// <summary>One action the application offers: the group it belongs to,
/// the label naming it, and its identifying number. Front ends express the
/// catalog of actions in their own way; a console shows a numbered menu
/// and reads the number back, while a web page could render the same
/// actions as buttons or links. The record describes the capability, not
/// its presentation.</summary>
public record AppAction(string GeneralClassification, string Label, int OptionNumber);
