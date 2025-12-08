using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

public abstract class StrategyBase
{
    public virtual string Name { get; protected set; }
    public abstract string NameOf { get; protected set; }
    public Spot Spot { get; set; }
}
