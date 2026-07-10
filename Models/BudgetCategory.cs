namespace BudgetTracker.Models;

public class BudgetCategory
{
    public BudgetCategory(string generalClassification, string categoryName, string[] keywords,
                      int optionNumber, decimal amtBudgeted, decimal searchOrder)
    {
        GeneralClassification = generalClassification;
        CategoryName = categoryName;
        Keywords = keywords;
        OptionNumber = optionNumber;
        AmtBudgeted = amtBudgeted;
        SearchOrder = searchOrder;
    }
        
    public string GeneralClassification { get; set; }
    public string CategoryName { get; set; }
    public string[] Keywords { get; set; }
    private int _optionNumber;
    public int OptionNumber
    {
        get => _optionNumber;
        set
        {
            if (value < 0)
                throw new ArgumentException("Option number must be a positive integer");
            _optionNumber = value;
        }
    }

    private decimal _amtBudgeted;
    public decimal AmtBudgeted
    {
        get => _amtBudgeted;
        set
        {
            if (value < 0)
                throw new ArgumentException("Budgeted amount must be non-negative");
            _amtBudgeted = Math.Round(value, 2);
        }
    }

    public decimal SearchOrder { get; set; }


}