using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

public class StrategyBundle<TTradeStrategy, TBudgetStrategy> : StrategyBundleBase
    where TTradeStrategy : TradeStrategyBase
    where TBudgetStrategy : BudgetStrategy
{
    public StrategyBundle(CacheService cache, StrategyVariation? strategyVariation = null)
    {
        TradeStrategy = (TTradeStrategy)Activator.CreateInstance(typeof(TTradeStrategy), cache, strategyVariation)!;
        BudgetStrategy = Activator.CreateInstance<TBudgetStrategy>()!;
        IsVariationBundle = strategyVariation != null;
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

public class StrategyBundle : StrategyBundleBase
{
    public StrategyBundle(TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy)
    {
        TradeStrategy = tradeStrategy;
        BudgetStrategy = budgetStrategy;
    }
}

public abstract class StrategyBundleBase
{
    public TradeStrategyBase TradeStrategy { get; protected set; }
    public BudgetStrategy BudgetStrategy { get; protected set; }
    public bool IsVariationBundle { get; set; } = false;
}