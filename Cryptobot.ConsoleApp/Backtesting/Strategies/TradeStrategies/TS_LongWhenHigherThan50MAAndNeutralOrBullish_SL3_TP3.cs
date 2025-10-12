using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_LongWhenHigherThan50MAAndNeutralOrBullish_SL3_TP3 : TradeStrategyBase
{
    public override string Name { get; protected set; } = "Trade when price higher than last 50 MA and Neutral or Bullish | SL -3% | TP +3%";
    public override string NameOf { get; protected set; } = nameof(TS_LongWhenHigherThan50MAAndNeutralOrBullish_SL3_TP3);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [IndicatorType.MovingAverage, IndicatorType.MicroTrend];

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.97;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.03;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.08;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.93;

    protected override bool ShouldShort<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex) => false;

    protected override bool ShouldLong<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex)
    {
        var nMABackCheck = 50;
        var candle = candles[currentCandleIndex];

        if (currentCandleIndex < nMABackCheck)
        {
            return false;
        }

        var lastN = candles.Skip(currentCandleIndex - nMABackCheck).Take(nMABackCheck).Select(x => x.Indicators.MovingAverage).ToArray();
        return lastN.All(x => x < candle.ClosePrice) && (candle.Indicators.MicroTrend is Trend.Neutral or Trend.Bull);
    }
}
