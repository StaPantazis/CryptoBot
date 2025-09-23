using Cryptobot.ConsoleApp.Bybit.Models;

namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public abstract class TradeStrategy
{
    public Spot Spot { get; set; }

    public virtual bool ShouldOpenTrade(List<BybitCandle> candles, int currentCandleIndex) => Spot.Budget > 10;

    public virtual bool ShouldCloseTrade(List<BybitCandle> candles, int currentCandleIndex, Trade trade)
    {
        var candle = candles[currentCandleIndex];
        return candle.LowPrice <= trade.StopLoss || candle.HighPrice >= trade.TakeProfit;
    }
}
