namespace Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;

public class BudgetStrategy_OnePercent : BudgetStrategy
{
    public override string Name { get; } = "Bet 1% of the budget";

    public override double DefineTradeSize() => Spot.Budget * 0.01;
}
