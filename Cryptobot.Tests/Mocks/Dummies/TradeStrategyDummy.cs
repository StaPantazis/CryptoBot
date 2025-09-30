using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.Tests.Mocks.Dummies;
internal class TradeStrategyDummy(PositionSide position) : TradeStrategy
{
    private readonly PositionSide _position = position;
    public override string Name { get; } = "Trade Dummy";
    public override string NameOf { get; } = nameof(TradeStrategyDummy);

    protected override double StopLossLong<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override double StopLossShort<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 1.05;
    public override bool ShouldCloseTrade<T>(List<T> candles, int i, Trade trade) => true;
    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex) => _position is PositionSide.Long;
    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex) => _position is PositionSide.Short;
}
