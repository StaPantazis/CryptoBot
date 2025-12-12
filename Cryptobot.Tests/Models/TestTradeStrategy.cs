using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.Tests.Builders;

namespace Cryptobot.Tests.Models;

public class TestTradeStrategy(bool longIfBudget, bool shortIfBudget, CandleInterval? candleInterval = null, StrategyVariation? variation = null)
    : TradeStrategyBase(A.Cache(), candleInterval ?? CandleInterval.Five_Minutes, variation)
{
    public override string NameOf { get; protected set; }
    protected override string NameOverridable { get; set; }

    protected override double? StopLossLong<T>(CandleSlice<T> slice) => 1;
    protected override double? StopLossShort<T>(CandleSlice<T> slice) => 1;
    protected override double? TakeProfitLong<T>(CandleSlice<T> slice) => 1;
    protected override double? TakeProfitShort<T>(CandleSlice<T> slice) => 1;

    protected override bool ShouldLong<T>(CandleSlice<T> slice) => longIfBudget;
    protected override bool ShouldShort<T>(CandleSlice<T> slice) => shortIfBudget;
}
