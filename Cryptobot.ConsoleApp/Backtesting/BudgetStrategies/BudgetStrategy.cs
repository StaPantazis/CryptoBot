namespace Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;

public abstract class BudgetStrategy
{
    public Spot Spot { get; set; }

    public abstract double DefineTradeSize();
}
