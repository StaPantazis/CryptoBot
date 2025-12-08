using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Indicators
{
    public double? MovingAverage { get; set; }
    public Trend? MovingAverageTrend { get; set; }
    public Trend? AiTrend { get; set; }
}
