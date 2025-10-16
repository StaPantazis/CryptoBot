using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public record StrategyVariationsBundle<T>(
    Func<List<T>, int, CandleInterval, (int? movingAverageNBack, List<Trend?>? applicableTrends), bool> ShouldLong,
    Func<List<T>, int, CandleInterval, (int? movingAverageNBack, List<Trend?>? applicableTrends), bool> ShouldShort,
    int?[]? MovingAverageNBack = null,
    double?[]? StopLossLong = null,
    double?[]? TakeProfitLong = null,
    double?[]? StopLossShort = null,
    double?[]? TakeProfitShort = null,
    List<IndicatorType?[]>? IndicatorTypes = null,
    List<List<Trend?>>? ApplicableTrends = null) where T : Candle;