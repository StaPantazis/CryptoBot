using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

namespace Cryptobot.Tests.Mocks.Dummies;
internal class TradeStrategyDummy : TradeStrategy
{
    public override string Name { get; } = "Trade Dummy";
    public override string NameOf { get; } = nameof(TradeStrategyDummy);

    public override double StopLoss<T>(List<T> candles, int currentCandleIndex) => throw new NotImplementedException();
    public override double TakeProfit<T>(List<T> candles, int currentCandleIndex) => throw new NotImplementedException();
    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex) => throw new NotImplementedException();
    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex) => throw new NotImplementedException();
}
