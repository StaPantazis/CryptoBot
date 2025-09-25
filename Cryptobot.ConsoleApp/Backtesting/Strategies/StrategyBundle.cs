using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

public class StrategyBundle<TTradeStrategy, TBudgetStrategy> : StrategyBundleBase
    where TTradeStrategy : TradeStrategy
    where TBudgetStrategy : BudgetStrategy
{
    public StrategyBundle()
    {
        TradeStrategy = Activator.CreateInstance<TTradeStrategy>()!;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
    }
}

public abstract class StrategyBundleBase
{
    public TradeStrategy TradeStrategy { get; init; }
    public BudgetStrategy BudgetStrategy { get; init; }
}