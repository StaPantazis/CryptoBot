using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.Tests.Builders;

namespace Cryptobot.Tests.Models;

public class TestTradeStrategy(bool longIfBudget, bool shortIfBudget, StrategyVariation? variation = null) : TradeStrategyBase(A.Cache(), variation)
{
    public override string NameOf { get; protected set; }
    protected override string NameOverridable { get; set; }

    protected override double? StopLossLong<T>(List<T> candles, int currentCandleIndex) => 1;
    protected override double? StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1;
    protected override double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1;
    protected override double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 1;

    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval) => longIfBudget;
    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval) => shortIfBudget;
}
