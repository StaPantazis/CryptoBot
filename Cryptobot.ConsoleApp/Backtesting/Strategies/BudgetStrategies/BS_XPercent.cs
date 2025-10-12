namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public class BS_XPercent(double percent) : BudgetStrategy
{
    private readonly double _percent = Math.Min(percent, 99);

    public override string Name { get; protected set; } = $"Bet {Math.Min(percent, 99)}% of the budget";
    public override string NameOf { get; protected set; } = nameof(BS_XPercent);

    public override double DefineTradeSize() => Spot.Budget * _percent / 100;
}
