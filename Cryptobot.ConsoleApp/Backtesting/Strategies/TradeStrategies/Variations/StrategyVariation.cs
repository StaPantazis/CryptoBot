using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

/// <summary>
/// Parameters: Low - High - High - Low
/// </summary>
/// <param name="StopLossLong">Low</param>
/// <param name="TakeProfitLong">High</param>
/// <param name="StopLossShort">High</param>
/// <param name="TakeProfitShort">Low</param>
/// <param name="MicroTrendConfig"></param>
public record StrategyVariation(
    double? StopLossLong = null,
    double? TakeProfitLong = null,
    double? StopLossShort = null,
    double? TakeProfitShort = null,
    TrendConfiguration? MicroTrendConfig = null)
{
    public string Name
    {
        get
        {
            var parts = new List<string>();

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

            return string.Join(" | ", parts);
        }
    }
};
