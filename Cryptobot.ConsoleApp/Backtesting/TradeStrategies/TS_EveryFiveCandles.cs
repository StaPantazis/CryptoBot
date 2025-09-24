namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TS_EveryFiveCandles : TradeStrategy
{
    public override string Name { get; } = "Trade every 5 candles";
    public override string NameOf { get; } = nameof(TS_EveryFiveCandles);

    public override bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex)
    {
        return currentCandleIndex % 5 == 0
            && base.ShouldOpenTrade(candles, currentCandleIndex);
    }
}
