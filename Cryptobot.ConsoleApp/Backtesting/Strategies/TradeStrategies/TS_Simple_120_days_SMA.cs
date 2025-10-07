using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Simple_120_days_SMA : TradeStrategyBase
{
    public override string Name { get; protected set; } = "Trade when higher than macro trend, sell when lower than macro trend | SL -3% | TP +3%";
    public override string NameOf { get; protected set; } = nameof(TS_Simple_120_days_SMA);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [IndicatorType.MovingAverage, IndicatorType.MacroTrend];

    protected override double StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.01;
    protected override double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 500.00;

    protected override double StopLossShort<T>(List<T> candles, int currentCandleIndex) => 500.00;
    protected override double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.01;

    public override bool ShouldExitLongTrade<T>(CacheManager cacheManager, List<T> candles, int currentCandleIndex, Trade trade)
    {
        if (currentCandleIndex < 11520)
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        var lala = currentCandle.ClosePrice < cacheManager.MacroTrendCache[currentCandle.OpenTime.Ticks].MA11520;

        if (lala is true)
        {

        }

        return lala;
    }

    protected override bool ShouldShort<T>(CacheManager cacheManager, List<T> candles, int currentCandleIndex) => false;

    protected override bool ShouldLong<T>(CacheManager cacheManager, List<T> candles, int currentCandleIndex)
    {
        if (currentCandleIndex < 11520)
        {
            return false;
        }

        var candle = candles[currentCandleIndex];
        return candle.ClosePrice > cacheManager.MacroTrendCache[candle.OpenTime.Ticks].MA11520;
    }
}
