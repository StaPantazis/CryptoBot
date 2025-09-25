using Cryptobot.ConsoleApp.Models;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public abstract class TradeStrategy : StrategyBase
{
    public abstract double StopLoss<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    public abstract double TakeProfit<T>(List<T> candles, int currentCandleIndex) where T : Candle;

    public virtual bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex) where T : Candle
        => Spot.Budget > 10;

    public virtual bool ShouldCloseTrade<T>(List<T> candles, int currentCandleIndex, Trade trade) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        return candle.LowPrice <= trade.StopLoss || candle.HighPrice >= trade.TakeProfit;
    }
}
