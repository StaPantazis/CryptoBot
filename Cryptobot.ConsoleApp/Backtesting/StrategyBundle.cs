using Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

namespace Cryptobot.ConsoleApp.Backtesting;

public class StrategyBundle<TTradeStrategy, TBudgetStrategy> : StrategyBundleBase
    where TTradeStrategy : TradeStrategy
    where TBudgetStrategy : BudgetStrategy
{
    public StrategyBundle()
    {
        TradeStrategy = Activator.CreateInstance<TTradeStrategy>()!;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
    }

    public TradeStrategy TradeStrategy { get; init; }
    public BudgetStrategy BudgetStrategy { get; init; }
}

public abstract class StrategyBundleBase;