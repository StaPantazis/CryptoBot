using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Aggressive_Trend_buy_green_sell_red(CacheService cache, StrategyVariation? variation = null) : TradeStrategyBase(cache, variation)
{
    protected override string NameOverridable { get; set; } = "Buy on green, sell on red";
    public override string NameOf { get; protected set; } = nameof(TS_Aggressive_Trend_buy_green_sell_red);
    public override IndicatorType[] RelevantIndicators { get; } = [IndicatorType.AiTrend];
    protected override AiTrendConfiguration AiTrendConfigOverridable => AiTrendConfiguration.Create(AiTrendProfile.Aggressive);

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.98;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.02;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.02;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.98;

    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        var candle = candles[currentCandleIndex];
        return candle.Indicators.AiTrend is Trend.Bull or Trend.FullBull;
    }

    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        var candle = candles[currentCandleIndex];
        return candle.Indicators.AiTrend is Trend.Bear or Trend.FullBear;
    }
}
