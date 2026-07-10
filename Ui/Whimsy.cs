using Figgle.Fonts;

namespace BudgetTracker.Ui;

public static class Whimsy
{    
    private static readonly string[] CowPhrases = [    
        "Let's see how your finances are moooooving!",
        "Time to mooooove some numbers around!",
        "Holy cow, let's see those numbers!",
        "I 'herd' you have budgets to explore!",
        "I'm 'udderly' excited to explore these budgets!",
        "Hay there, budget-wrangler!",
        "I find finances a-moosing!",
        "Here to moo-tivate you!",
        "Budgeting is udderly moo-velous!",
        "Numbers put me in a happy moo-d!",
        "Let's cow-laborate on these budgets!",
        "Budgets can be udderly hilarious! Or at least mildly a-moosing.",
        "Exploring budgets cow-nts as fun, right?",
        "Time to get off the cow-ch and chew some numbers!",
        "Our cow-laboration is legen-dairy!",
        "The moo-ment has come to explore your finances!"
    ];
    public static string ApplyWhimsy(bool enableWhimsy = true)
    {
        if (!enableWhimsy) return "\n\nWelcome to the Budget Tracker\n\n";

        string title = FiggleFonts.Banner.Render("Budget Tracker");
        string cowPhrase = CowPhrases[Random.Shared.Next(CowPhrases.Length)];
        string cowified = CowQuip.Say(cowPhrase);
        return $"\n\n{title}\n{cowified}\n\n";
    }
}