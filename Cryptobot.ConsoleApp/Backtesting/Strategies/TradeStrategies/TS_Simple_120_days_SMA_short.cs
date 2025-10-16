using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Simple_120_days_SMA_short : TradeStrategyBase
{
    public override string Name { get; protected set; } = "Sell when lower than macro trend (SHORT)";
    public override string NameOf { get; protected set; } = nameof(TS_Simple_120_days_SMA_short);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [IndicatorType.MovingAverage, IndicatorType.MacroTrend];

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => null;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => null;

    public override bool ShouldExitShortTrade<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex, Trade trade, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        return currentCandle.ClosePrice > cacheManager.MacroTrendCache[currentCandle.DayTicks].MA120d;
    }

    protected override bool ShouldShort<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        return currentCandle.ClosePrice < cacheManager.MacroTrendCache[currentCandle.DayTicks].MA120d;
    }
}
