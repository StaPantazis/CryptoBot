using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Simple_120_days_SMA_short(CacheService cacheService, StrategyVariation? variation = null) : TradeStrategyBase(cacheService, variation)
{
    public override string Name { get; protected set; } = "Sell when lower than macro trend (SHORT)";
    public override string NameOf { get; protected set; } = nameof(TS_Simple_120_days_SMA_short);
    public override IndicatorType[] RelevantIndicators { get; } = [IndicatorType.MovingAverage, IndicatorType.MacroTrend];

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => null;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => null;

    public override bool ShouldExitShortTrade<T>(List<T> candles, int currentCandleIndex, Trade trade, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        return currentCandle.ClosePrice > Cache.MacroTrendCache[currentCandle.DayKey].MovingAverage;
    }

    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        return currentCandle.ClosePrice < Cache.MacroTrendCache[currentCandle.DayKey].MovingAverage;
    }
}
