using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

public class StrategyBundle<TTradeStrategy, TBudgetStrategy> : StrategyBundleBase
    where TTradeStrategy : TradeStrategyBase
    where TBudgetStrategy : BudgetStrategy
{
    public StrategyBundle()
    {
        TradeStrategy = Activator.CreateInstance<TTradeStrategy>()!;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
    }

    public StrategyBundle(TradeStrategyBase tradeStrategy)
    {
        TradeStrategy = tradeStrategy;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
    }
}

public class StrategyBundle<TBudgetStrategy> : StrategyBundleBase
    where TBudgetStrategy : BudgetStrategy
{
    public StrategyBundle(TradeStrategyBase tradeStrategy)
    {
        TradeStrategy = tradeStrategy;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
    }
}

public abstract class StrategyBundleBase
{
    public TradeStrategyBase TradeStrategy { get; protected set; }
    public BudgetStrategy BudgetStrategy { get; protected set; }
}