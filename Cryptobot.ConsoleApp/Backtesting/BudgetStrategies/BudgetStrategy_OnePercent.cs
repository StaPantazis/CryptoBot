namespace Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;

public class BudgetStrategy_OnePercent : BudgetStrategy
{
    public override double DefineTradeSize() => Spot.Budget * 0.01;
}
