using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public abstract class TrendTradeStrategy : TradeStrategy
{
    public virtual TrendConfiguration TrendConfiguration { get; } = new(window: 30);
}
