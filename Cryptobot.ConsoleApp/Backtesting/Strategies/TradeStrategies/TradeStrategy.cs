using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public abstract class TradeStrategy : StrategyBase
{
    public abstract double StopLoss<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    public abstract double TakeProfit<T>(List<T> candles, int currentCandleIndex) where T : Candle;

    public virtual bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex, out PositionSide? position) where T : Candle
    {
        position = ShouldShort(candles, currentCandleIndex) ? PositionSide.Short
                 : ShouldLong(candles, currentCandleIndex) ? PositionSide.Long
                 : null;

        return Spot.Budget > 10;
    }

    public virtual bool ShouldCloseTrade<T>(List<T> candles, int currentCandleIndex, Trade trade) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        return candle.LowPrice <= trade.StopLoss || candle.HighPrice >= trade.TakeProfit;
    }

    protected abstract bool ShouldShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract bool ShouldLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
}
