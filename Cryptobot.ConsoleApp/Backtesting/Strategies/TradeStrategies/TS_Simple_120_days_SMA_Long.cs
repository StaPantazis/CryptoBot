using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Simple_120_days_SMA_Long(CacheService cache, CandleInterval tradingCandleInterval, StrategyVariation? variation = null)
    : TradeStrategyBase(cache, tradingCandleInterval, variation)
{
    protected override string NameOverridable { get; set; } = "Sell when higher (LONG)";
    public override string NameOf { get; protected set; } = nameof(TS_Simple_120_days_SMA_Long);
    public override IndicatorType[] RelevantIndicators { get; } = [IndicatorType.MovingAverage];

    protected override double? StopLossLong<T>(CandleSlice<T> slice) => null;
    protected override double? StopLossShort<T>(CandleSlice<T> slice) => null;
    protected override double? TakeProfitLong<T>(CandleSlice<T> slice) => null;
    protected override double? TakeProfitShort<T>(CandleSlice<T> slice) => null;

    public override bool ShouldExitLongTrade<T>(CandleSlice<T> slice, Trade trade)
    {
        if (slice.LiveCandle.DayKey < Constants.DAY_KEY_120_DAYS_AFTER_MARCH_26)
        {
            return false;
        }

        var lastCandle = slice.LastCandle;
        return lastCandle.OpenPrice > lastCandle.Indicators.MovingAverage && lastCandle.LowPrice < lastCandle.Indicators.MovingAverage;
    }

    protected override bool ShouldLong<T>(CandleSlice<T> slice)
    {
        if (slice.LiveCandle.DayKey < Constants.DAY_KEY_120_DAYS_AFTER_MARCH_26)
        {
            return false;
        }

        var lastCandle = slice.LastCandle;
        return lastCandle.OpenPrice < lastCandle.Indicators.MovingAverage && lastCandle.HighPrice > lastCandle.Indicators.MovingAverage;
    }
}
