namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

internal class BS_Steady100_OnlyOneTrade : BudgetStrategy
{
    public override string Name { get; protected set; } = $"Bet 100€ every time";
    public override string NameOf { get; protected set; } = nameof(BS_Steady100_OnlyOneTrade);

    public override double DefineTradeSize() => Spot.OpenTrades.Any() ? 0 : 100;
}