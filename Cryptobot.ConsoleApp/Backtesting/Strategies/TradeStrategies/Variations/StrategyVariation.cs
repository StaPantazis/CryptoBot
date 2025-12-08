using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

/// <summary>
/// Parameters: Low - High - High - Low
/// </summary>
/// <param name="StopLossLong">Low</param>
/// <param name="TakeProfitLong">High</param>
/// <param name="StopLossShort">High</param>
/// <param name="TakeProfitShort">Low</param>
/// <param name="AiTrendConfig"></param>
public record StrategyVariation(
    double? StopLossLong = null,
    double? TakeProfitLong = null,
    double? StopLossShort = null,
    double? TakeProfitShort = null,
    AiTrendConfiguration? AiTrendConfig = null)
{
    public string Name
    {
        get
        {
            var parts = new List<string>();

            if (StopLossLong != null)
            {
                parts.Add($"SL_Long:{StopLossLong}");
            }

            if (TakeProfitLong != null)
            {
                parts.Add($"TP_Long:{TakeProfitLong}");
            }

            if (StopLossShort != null)
            {
                parts.Add($"SL_Short:{StopLossShort}");
            }

            if (TakeProfitShort != null)
            {
                parts.Add($"TP_Short:{TakeProfitShort}");
            }

            if (AiTrendConfig != null)
            {
                parts.Add($"AI Trend Config:{AiTrendConfig}");
            }

            return string.Join(" | ", parts);
        }
    }
};
