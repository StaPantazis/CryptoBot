using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Indicators
{
    public double? MovingAverage { get; set; }
    public Trend? MicroTrend { get; set; }
    public Trend? SemiTrend { get; set; }
    public Trend? MacroTrend { get; set; }
}
