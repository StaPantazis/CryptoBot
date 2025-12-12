using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.Tests.Models;

namespace Cryptobot.Tests.Builders;

public static class A
{
    private const double _budget = 100;

    public static Candle Candle(double open, double high, double low, double close, string? id = null) => new TestCandle(id)
    {
        OpenTime = DateTime.UtcNow,
        OpenPrice = open,
        HighPrice = high,
        LowPrice = low,
        ClosePrice = close,
    };
    public static List<Candle> Candles(params Candle[] candles) => candles.ToList();
    public static CandleSlice<Candle> Slice(int? size) => new(size ?? 1);
    public static CandleSlice<Candle> Slice(params Candle[] candles) => new(candles.ToList());
    public static TradeStrategyBuilder TradeStrategy() => new();
    public static CacheService Cache() => new();
    public static Spot Spot(TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy, FeesSettings? feesSettings = null)
        => new(new User("Test User"), _budget, tradeStrategy, budgetStrategy, feesSettings ?? new(tradeFeeOverride: 0, slippageOverride: 0));
}
