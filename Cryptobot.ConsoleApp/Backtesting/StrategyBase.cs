namespace Cryptobot.ConsoleApp.Backtesting;

public abstract class StrategyBase
{
    public abstract string Name { get; }
    public abstract string NameOf { get; }
    public Spot Spot { get; set; }
}
