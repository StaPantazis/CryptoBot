using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Every100Candles_SL5_TP5 : TradeStrategyBase
{
    public override string Name { get; protected set; } = "Trade every 100 candles | SL -5% | TP +5%";
    public override string NameOf { get; protected set; } = nameof(TS_Every100Candles_SL5_TP5);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [];

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.05;

    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.08;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.93;

    protected override bool ShouldShort<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex) => currentCandleIndex % 400 == 0;

    protected override bool ShouldLong<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex) => currentCandleIndex % 400 != 0 && currentCandleIndex % 100 == 0;
}
