using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

namespace Cryptobot.Tests.Mocks.Dummies;

internal class BudgetStrategyDummy : BudgetStrategy
{
    public override string Name { get; protected set; } = "Budget Dummy";
    public override string NameOf { get; protected set; } = nameof(BudgetStrategyDummy);

    public override double DefineTradeSize() => Spot.AvailableBudget * 0.1;
}
