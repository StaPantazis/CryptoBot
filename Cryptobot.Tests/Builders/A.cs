using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.Tests.Models;

namespace Cryptobot.Tests.Builders;

public static class A
{
    private const double _budget = 100;

    public static Candle Candle(int id) => Candle(open: null, high: null, low: null, close: null, id: id);
    public static Candle Candle(double? open = null, double? high = null, double? low = null, double? close = null, int? id = null) => new TestCandle(id.ToString())
    {
        OpenTime = DateTime.UtcNow,
        OpenPrice = open ?? 100,
        HighPrice = high ?? 100,
        LowPrice = low ?? 100,
        ClosePrice = close ?? 100,
    };
    public static List<Candle> Candles(int defaultCandlesCount) => Candles(Enumerable.Range(1, defaultCandlesCount).Select(i => Candle(id: i)).ToArray());
    public static List<Candle> Candles(params Candle[] candles) => candles.ToList();
    public static CandleSlice<Candle> Slice(int? size) => new(size ?? 1);
    public static CandleSlice<Candle> Slice(params Candle[] candles) => new(candles.ToList());
    public static TradeStrategyBuilder TradeStrategy() => new();
    public static CacheService Cache() => new();
    public static Spot Spot(TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy, FeesSettings? feesSettings = null)
        => new(new User("Test User"), _budget, tradeStrategy, budgetStrategy, feesSettings ?? new(tradeFeeOverride: 0, slippageOverride: 0));
}
