namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public class BS_XPercent(double percent) : BudgetStrategy
{
    public override string Name { get; protected set; } = $"Bet {percent}% of the budget";
    public override string NameOf { get; protected set; } = nameof(BS_XPercent);

    public override double DefineTradeSize() => Spot.Budget * percent / 100;
}
