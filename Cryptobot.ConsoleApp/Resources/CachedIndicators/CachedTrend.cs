using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators;

public abstract class CachedTrend(DateTime openDateTime, Trend trend)
{
    public abstract long Key { get; }

    public DateTime OpenDateTime { get; } = openDateTime;
    public Trend Trend { get; } = trend;

    public override string ToString() => $"{OpenDateTime:dd/MM/yyyy} - {Trend}";
}

public class CachedMacroTrend(DateTime openDateTime, double? movingAverage, Trend trend) : CachedTrend(openDateTime, trend)
{
    public double? MovingAverage { get; } = movingAverage;
    public override long Key { get; } = openDateTime.BuildDayKey();

    public override string ToString() => $"{base.ToString()} - {MovingAverage?.ToString() ?? "N/A"}";
}
public class CachedTrendProfileAI_Balanced(DateTime openDateTime, Trend trend) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = openDateTime.BuildFifteenMinuteKey();
}
public class CachedTrendProfileAI_Conservative(DateTime openDateTime, Trend trend) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = openDateTime.BuildFifteenMinuteKey();
}
public class CachedTrendProfileAI_Aggressive(DateTime openDateTime, Trend trend) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = openDateTime.BuildFifteenMinuteKey();
}
public class CachedTrendProfileAI_Default(DateTime openDateTime, Trend trend) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = openDateTime.BuildFifteenMinuteKey();
}