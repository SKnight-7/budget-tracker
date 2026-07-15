namespace BudgetTracker.Ui;

/// <summary>One entry in a menu: the group it displays under, the text shown
/// beside its number, and the number a user types to select it.</summary>
public record MenuOption(string GeneralClassification, string Label, int OptionNumber);