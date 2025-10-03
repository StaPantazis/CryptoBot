using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.OutputModels;

public class TrendCandle : Candle
{
    public Trend Trend { get; set; }
}
