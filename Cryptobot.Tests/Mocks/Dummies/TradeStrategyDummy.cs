using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.Tests.Mocks.Dummies;

internal class TradeStrategyDummy(PositionSide position, CacheService cacheService, StrategyVariation? variation = null)
        : TradeStrategyBase(cacheService, variation)
{
    private readonly PositionSide _position = position;
    public override string Name { get; protected set; } = "Trade Dummy";
    public override string NameOf { get; protected set; } = nameof(TradeStrategyDummy);

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex, CandleInterval _) => _position is PositionSide.Long;
    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex, CandleInterval _) => _position is PositionSide.Short;
}
