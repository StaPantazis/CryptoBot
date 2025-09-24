namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TradeStrategy_EveryFiveCandles : TradeStrategy
{
    public override string Name { get; } = "Trade every 5 candles";

    public override bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex)
    {
        return currentCandleIndex % 5 == 0
            //&& Spot.Trades.Count < 100
            && base.ShouldOpenTrade(candles, currentCandleIndex);
    }
}
