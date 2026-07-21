using BudgetTracker.Models;

namespace BudgetTracker.Data;

/// <summary>
/// The built-in starter set of budget categories: the template every fresh
/// installation begins from.
/// </summary>
public static class DefaultCategories
{
    /// <summary>Builds a fresh set of the twenty default categories.</summary>
    /// <returns>New BudgetCategory objects on every call, so callers can modify
    /// their copies without affecting anyone else's.</returns>
    /// <remarks>Some keywords carry deliberate leading or trailing spaces
    /// (" rt ", "cat ") as word-boundary guards for the categorizer's substring
    /// matching. The spaces are load-bearing; edit with care.</remarks>
    public static List<BudgetCategory> GetDefaults() =>
    // Each entry: (GeneralClassification, Name, Keywords, OptionNumber, AmtBudgeted, SearchOrder)
    [
        new("Income", "Paycheck", ["payroll"], 1, 0m, 1),
        new("Income", "Other Income", ["cashout"], 2, 0m, 2),
        new("Monthly Household", "Mortgage & Rent", ["apartments", "mortgage"], 3, 0m, 3),
        new("Monthly Household", "Utilities", ["utility", "gas", "electric", "water", "smud", "pge"], 4, 0m, 4),
        new("Monthly Household", "Phone", ["verizon", "metropcs", "mobile"], 5, 0m, 5),
        new("Monthly Household", "Internet, Cable, Satellite", ["internet", "comcast", "xfinity", "at&t", "cable", "satellite"], 6, 0m, 6),
        new("Food & Dining", "Groceries", ["safeway", "kroger", "aldi", "publix", "meijer", "piggly", "albertson", "costco", "trader joe", "co-op", "food", "market", "grocery"], 7, 0m, 7),
        new("Food & Dining", "Eating Out", ["mcdonald", "starbuck", "peets", "chipotle", "subway", "panera", "dunkin", "taco", "pizza", "wings", "burger", "steak", "coffee", "yogurt"], 8, 0m, 8),
        new("Travel & Transport", "Car (Payment, Gas, Repair, Ride Share, Tolls, Parking)", ["dealership", "auto", "uber", "lyft", "toll", "parking", "shell", "chevron", "exxonmobil", "bp", "gas"], 9, 0m, 9),
        new("Travel & Transport", "Public Transit", ["transit", " rt "], 10, 0m, 10),
        new("Travel & Transport", "Trips & Travel", ["hotel", "motel", "airline"], 11, 0m, 14),
        new("Health & Fitness", "Medical", ["hospital", "doctor", "kaiser", "medical", "insurance", "wellness", "pharm", "rx"], 12, 0m, 17),
        new("Health & Fitness", "Gym & Other Fitness", ["fitness", "gym", "pilates", "dance", "running"], 13, 0m, 13),
        new("Financial", "Pay Loans & Credit Cards", ["bank", "loan", "capital one", "merrick", "hsbc", "american express", "visa", "mastercard", "student ln", "synchrony", " cc "], 14, 0m, 11),
        new("Shopping", "Home Improvement", ["lowe", "home", "hardware"], 15, 0m, 15),
        // Search order 999: effectively last, so broad keywords like "outlet" and
        // "amazon" can't steal matches from more specific categories.
        new("Shopping", "Other Shopping", ["amazon", "amzn", "ebay", "macy", "nordstrom", "target", "walmart", "outlet", "google"], 16, 0m, 999),
        new("Other", "Self Care", ["spa ", " hair", "nail", "salon", "barber", "massage", "beauty"], 17, 0m, 16),
        new("Other", "Pet Care", ["chewy", "animal", "vet", "kitty", "cat ", "dog", "hound", "pup"], 18, 0m, 12),
        new("Other", "Laundry", ["csc"], 19, 0m, 19),
        new("Other", "Entertainment", ["netflix", "hulu", "disney", "video", "spotify", "audible", "cinemark", "amc", "theater", "theatre", "playstation", "nintendo", "xbox", "steam", "nexus mods", "game", "subscription", "youtube", "channel", "television", "tv"], 20, 0m, 20),
    ];
}
