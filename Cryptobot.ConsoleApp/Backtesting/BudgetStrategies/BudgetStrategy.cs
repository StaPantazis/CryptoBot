namespace Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;

public abstract class BudgetStrategy : StrategyBase
{
    public abstract double DefineTradeSize();
}
