using Cryptobot.ConsoleApp.Bybit.Models;

namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TradeStrategy_EveryFiveCandles : TradeStrategy
{
    public override bool ShouldOpenTrade(List<BybitCandle> candles, int currentCandleIndex)
        => currentCandleIndex % 5 == 0
        && Spot.Trades.Count < 100 &&
        base.ShouldOpenTrade(candles, currentCandleIndex);
}
