namespace Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;

public abstract class BudgetStrategy
{
    public abstract string Name { get; }
    public Spot Spot { get; set; }

    public abstract double DefineTradeSize();
}
