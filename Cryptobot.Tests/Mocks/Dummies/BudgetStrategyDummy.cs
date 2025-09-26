using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

namespace Cryptobot.Tests.Mocks.Dummies;

internal class BudgetStrategyDummy : BudgetStrategy
{
    public override string Name { get; } = "Budget Dummy";
    public override string NameOf { get; } = nameof(BudgetStrategyDummy);

    public override double DefineTradeSize() => throw new NotImplementedException();
}
