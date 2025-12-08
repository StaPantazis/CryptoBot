using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Simple_120_days_SMA_Long(CacheService cache, StrategyVariation? variation = null) : TradeStrategyBase(cache, variation)
{
    protected override string NameOverridable { get; set; } = "Sell when higher (LONG)";
    public override string NameOf { get; protected set; } = nameof(TS_Simple_120_days_SMA_short);
    public override IndicatorType[] RelevantIndicators { get; } = [IndicatorType.MovingAverage];

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => null;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => null;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => null;

    public override bool ShouldExitLongTrade<T>(List<T> candles, int currentCandleIndex, Trade trade, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var currentCandle = candles[currentCandleIndex];
        return currentCandle.ClosePrice < Cache.MovingAverageTrendCache[currentCandle.DayKey].MovingAverage;
    }

    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        if (currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
        {
            return false;
        }

        var candle = candles[currentCandleIndex];
        return candle.ClosePrice > Cache.MovingAverageTrendCache[candle.DayKey].MovingAverage;
    }
}
