namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public class BS_OnePercent : BudgetStrategy
{
    public override string Name { get; } = "Bet 1% of the budget";
    public override string NameOf { get; } = nameof(BS_OnePercent);

    public override double DefineTradeSize() => Spot.Budget * 0.01;
}
