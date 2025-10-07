using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public abstract class TradeStrategyBase : StrategyBase
{
    public virtual TrendConfiguration MicroTrendConfiguration { get; } = new(window: 30);
    public virtual TrendConfiguration SemiTrendConfiguration { get; } = new(window: 30);

    public abstract IndicatorType[] RelevantIndicators { get; protected set; }

    public double StopLoss<T>(List<T> candles, int currentCandleIndex, PositionSide position) where T : Candle
        => position is PositionSide.Long ? StopLossLong(candles, currentCandleIndex) : StopLossShort(candles, currentCandleIndex);

    public double TakeProfit<T>(List<T> candles, int currentCandleIndex, PositionSide position) where T : Candle
        => position is PositionSide.Long ? TakeProfitLong(candles, currentCandleIndex) : TakeProfitShort(candles, currentCandleIndex);

    public virtual bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex, out PositionSide? position) where T : Candle
    {
        if (Spot.Budget < 10)
        {
            position = null;
            return false;
        }

        position = ShouldShort(candles, currentCandleIndex) ? PositionSide.Short
                 : ShouldLong(candles, currentCandleIndex) ? PositionSide.Long
                 : null;

        return position != null;
    }

    public virtual bool ShouldCloseTrade<T>(List<T> candles, int currentCandleIndex, Trade trade) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        return (trade.PositionSide is PositionSide.Long && (candle.LowPrice <= trade.StopLoss || candle.HighPrice >= trade.TakeProfit))
            || (trade.PositionSide is PositionSide.Short && (candle.HighPrice >= trade.StopLoss || candle.LowPrice <= trade.TakeProfit));
    }

    protected abstract double StopLossLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double StopLossShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract bool ShouldShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract bool ShouldLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
}
