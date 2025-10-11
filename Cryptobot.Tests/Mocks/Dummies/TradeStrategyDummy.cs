using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.Tests.Mocks.Dummies;
internal class TradeStrategyDummy(PositionSide position) : TradeStrategyBase
{
    private readonly PositionSide _position = position;
    public override string Name { get; protected set; } = "Trade Dummy";
    public override string NameOf { get; protected set; } = nameof(TradeStrategyDummy);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [];

    protected override double StopLossLong<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override double StopLossShort<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 1.05;
    public override bool ShouldCloseTrade<T>(CacheService cacheManager, List<T> candles, int i, Trade trade) => true;
    protected override bool ShouldLong<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex) => _position is PositionSide.Long;
    protected override bool ShouldShort<T>(CacheService cacheManager, List<T> candles, int currentCandleIndex) => _position is PositionSide.Short;
}
