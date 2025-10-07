namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public class BS_100Percent : BudgetStrategy
{
    public override string Name { get; protected set; } = "Bet 100% of the budget";
    public override string NameOf { get; protected set; } = nameof(BS_100Percent);

    public override double DefineTradeSize() => Spot.Budget;
}
