using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Aggressive_Trend_buy_green_sell_red : TradeStrategyBase
{
    private IndicatorService? _indicatorService = null;

    public override string Name { get; protected set; } = "Buy on green, sell on red";
    public override string NameOf { get; protected set; } = nameof(TS_Aggressive_Trend_buy_green_sell_red);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [IndicatorType.MicroTrend];
    public override TrendConfiguration MicroTrendConfiguration => TrendConfiguration.Aggressive();

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.05;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.95;

    protected override bool ShouldLong<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        _indicatorService ??= new IndicatorService(cacheManager, this);

        var candle = candles[currentCandleIndex];
        _indicatorService.CalculateRelevantIndicators(candle, candles, currentCandleIndex);

        return candle.Indicators.MicroTrend is Trend.Bull;
    }

    protected override bool ShouldShort<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        _indicatorService ??= new IndicatorService(cacheManager, this);

        var candle = candles[currentCandleIndex];
        _indicatorService.CalculateRelevantIndicators(candle, candles, currentCandleIndex);

        return candle.Indicators.MicroTrend is Trend.Bear;
    }
}
