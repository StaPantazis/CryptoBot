using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public class MetricsBundle(Spot spot)
{
    public BacktestMetrics Full { get; } = new(spot.Trades, spot, "Full");
    public BacktestMetrics Long { get; } = new(spot.Trades.Where(x => x.PositionSide is PositionSide.Long).ToList(), spot, "Longs");
    public BacktestMetrics Short { get; } = new(spot.Trades.Where(x => x.PositionSide is PositionSide.Short).ToList(), spot, "Shorts");
}
