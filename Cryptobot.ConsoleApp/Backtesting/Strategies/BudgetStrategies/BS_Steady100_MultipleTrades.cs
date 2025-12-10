namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

internal class BS_Steady100_MultipleTrades : BudgetStrategy
{
    public override string Name { get; protected set; } = $"Bet 100€ every time";
    public override string NameOf { get; protected set; } = nameof(BS_Steady100_MultipleTrades);

    public override double DefineTradeSize() => 100;
}