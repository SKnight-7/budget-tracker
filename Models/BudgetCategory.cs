namespace BudgetTracker.Models;

public class BudgetCategory
{
    private decimal _amtBudgeted;
    public decimal AmtBudgeted
    {
        get {return _amtBudgeted;}
        set
        {
            if (value < 0)
                throw new ArgumentException("Budgeted amount must be non-negative");
            _amtBudgeted = value;
        }
    }
}