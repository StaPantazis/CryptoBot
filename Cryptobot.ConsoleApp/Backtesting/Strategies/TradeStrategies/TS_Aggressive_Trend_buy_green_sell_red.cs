using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Aggressive_Trend_buy_green_sell_red(CacheService cache, CandleInterval tradingCandleInterval, StrategyVariation? variation = null)
    : TradeStrategyBase(cache, tradingCandleInterval, variation, aiTrendInterval: CandleInterval.Fifteen_Minutes)
{
    protected override string NameOverridable { get; set; } = "Buy on green, sell on red";
    public override string NameOf { get; protected set; } = nameof(TS_Aggressive_Trend_buy_green_sell_red);
    public override IndicatorType[] RelevantIndicators { get; } = [IndicatorType.AiTrend];
    protected override AiTrendConfiguration AiTrendConfigOverridable => AiTrendConfiguration.Create(AiTrendProfile.Default);

    protected override double? StopLossLong<T>(CandleSlice<T> slice) => 0.97;
    protected override double? StopLossShort<T>(CandleSlice<T> slice) => 1.02;
    protected override double? TakeProfitLong<T>(CandleSlice<T> slice) => 1.02;
    protected override double? TakeProfitShort<T>(CandleSlice<T> slice) => 0.98;

    protected override bool ShouldLong<T>(CandleSlice<T> slice) => slice.LastCandle.Indicators.AiTrend is Trend.Bull or Trend.FullBull;

    protected override bool ShouldShort<T>(CandleSlice<T> slice) => slice.LastCandle.Indicators.AiTrend is Trend.Bear or Trend.FullBear;
}
