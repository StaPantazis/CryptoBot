using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators;

public abstract class CachedTrend(DateTime openDateTime, double? movingAverage, Trend trend)
{
    public abstract long Key { get; }

    public DateTime OpenDateTime { get; } = openDateTime;
    public double? MovingAverage { get; } = movingAverage;
    public Trend Trend { get; } = trend;

    public override string ToString() => $"{OpenDateTime:dd/MM/yyyy} - {Trend} - {MovingAverage?.ToString() ?? "N/A"}";
}

public class CachedMacroTrend(DateTime openDateTime, double? movingAverage, Trend trend) : CachedTrend(openDateTime, movingAverage, trend)
{
    public override long Key { get; } = openDateTime.BuildDayKey();
}
public class CachedSemiTrend(DateTime openDateTime, double? movingAverage, Trend trend) : CachedTrend(openDateTime, movingAverage, trend)
{
    public override long Key { get; } = openDateTime.BuildFifteenMinuteKey();
}