using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public static class TradeStrategyVariationFactory
{
    public static StrategyBundleBase[] Sandbox<TTradeStrategy, TBudgetStrategy>(CacheService cache)
        where TTradeStrategy : TradeStrategyBase
        where TBudgetStrategy : BudgetStrategy
    {
        var variations = new List<StrategyVariation>();

        //Low1To6Percent().ForEachDo(stopLossLong
        //    => High1To6Percent().ForEachDo(takeProfitLong
        //        => High1To6Percent().ForEachDo(stopLossShort
        //            => Low1To6Percent().ForEachDo(takeProfitShort
        //                => AllTrendStrategies().ForEachDo(trendConfig
        //                    => variations.Add(new(
        //                        StopLossLong: stopLossLong,
        //                        TakeProfitLong: takeProfitLong,
        //                        StopLossShort: stopLossShort,
        //                        TakeProfitShort: takeProfitShort,
        //                        MicroTrendConfig: trendConfig)))))));

        variations.Add(new StrategyVariation(0.98, 1.02, 1.02, 0.98, TrendConfiguration.Aggressive()));
        variations.Add(new StrategyVariation(0.97, 1.03, 1.03, 0.97, TrendConfiguration.Aggressive()));

        var bundle = variations
            .Select(x => new StrategyBundle<TBudgetStrategy>(
                tradeStrategy: (TS_Aggressive_Trend_buy_green_sell_red)Activator.CreateInstance(
                    type: typeof(TS_Aggressive_Trend_buy_green_sell_red),
                    args: [cache, x])!))
            .ToArray();

        return bundle;
    }

    private static double?[] ArrayLoops(params double?[]? nums) => nums is null || !nums.Any() ? [null] : nums;
    private static double[] Low1To6Percent() => ArrayLoops(0.94, 0.95, 0.96, 0.97, 0.98, 0.99).Cast<double>().ToArray();
    private static double[] High1To6Percent() => ArrayLoops(1.01, 1.02, 1.03, 1.04, 1.05, 1.06).Cast<double>().ToArray();
    private static TrendConfiguration[] AllTrendStrategies()
        => [TrendConfiguration.Default(), TrendConfiguration.Balanced(), TrendConfiguration.Conservative(), TrendConfiguration.Aggressive()];
}
