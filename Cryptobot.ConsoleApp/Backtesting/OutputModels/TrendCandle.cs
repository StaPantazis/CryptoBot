using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.OutputModels;

public class TrendCandle : BybitCandle
{
    public Trend Trend { get; set; }

    public override string ToString() => Trend.ToString();
}
