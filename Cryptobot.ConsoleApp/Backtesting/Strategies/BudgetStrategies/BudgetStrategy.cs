namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public abstract class BudgetStrategy : StrategyBase
{
    public abstract double DefineTradeSize();
}
