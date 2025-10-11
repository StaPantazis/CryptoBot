using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators;

public class CachedMacroTrend(DateTime openDatetime, double? ma120d, Trend trend)
{
    private readonly DateTime _openDatetime = openDatetime;

    public long DayTicks { get; set; } = openDatetime.Date.Ticks;
    public double? MA120d { get; set; } = ma120d;
    public Trend Trend { get; set; } = trend;

    public override string ToString() => $"{_openDatetime:dd/MM/yyyy} - {Trend} - {MA120d?.ToString() ?? "N/A"}";
}
