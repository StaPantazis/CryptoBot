using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public record StrategyVariation<T>(
    Func<List<T>, int, bool> ShouldLong,
    Func<List<T>, int, bool> ShouldShort,
    int? MovingAverage,
    double? StopLossLong,
    double? TakeProfitLong,
    double? StopLossShort,
    double? TakeProfitShort,
    IndicatorType[]? IndicatorTypes,
    Trend[]? ApplicableTrends = null) where T : Candle
{
    public string Name
    {
        get
        {
            var parts = new List<string>();

            if (MovingAverage is { } ma)
            {
                parts.Add($"MA:{ma}");
            }

            if (StopLossLong is { } slLong)
            {
                parts.Add($"SL_Long:{slLong}");
            }

            if (TakeProfitLong is { } tpLong)
            {
                parts.Add($"TP_Long:{tpLong}");
            }

            if (StopLossShort is { } slShort)
            {
                parts.Add($"SL_Short:{slShort}");
            }

            if (TakeProfitShort is { } tpShort)
            {
                parts.Add($"TP_Short:{tpShort}");
            }

            if (IndicatorTypes is { Length: > 0 })
            {
                parts.Add($"Indicators:[{string.Join(", ", IndicatorTypes)}]");
            }

            if (ApplicableTrends is { Length: > 0 })
            {
                parts.Add($"Applicable Trends:[{string.Join(", ", ApplicableTrends)}]");
            }

            return string.Join(" | ", parts);
        }
    }
};
