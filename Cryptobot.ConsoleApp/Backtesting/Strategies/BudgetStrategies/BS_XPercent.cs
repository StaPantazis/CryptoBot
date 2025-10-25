namespace Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;

public class BS_XPercent(double percent) : BudgetStrategy
{
    private readonly double _percent = Math.Min(percent, 99);

    public override string Name { get; protected set; } = $"Bet {Math.Min(percent, 99)}% of the budget";
    public override string NameOf { get; protected set; } = $"BS_{Math.Min(percent, 99)}Percent";

    public override double DefineTradeSize() => Spot.AvailableBudget * _percent / 100;
}

public class BS_1Percent() : BS_XPercent(1) { }
public class BS_2Percent() : BS_XPercent(2) { }
public class BS_3Percent() : BS_XPercent(3) { }
public class BS_5Percent() : BS_XPercent(5) { }
public class BS_10Percent() : BS_XPercent(10) { }
public class BS_20Percent() : BS_XPercent(20) { }
public class BS_100Percent() : BS_XPercent(100) { }