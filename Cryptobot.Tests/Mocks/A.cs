using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;
using Cryptobot.Tests.Mocks.Dummies;

namespace Cryptobot.Tests.Mocks;

public static class A
{
    private const double _budget = 1_000_000;

    public static Candle Candle(double open, double high, double low, double close) => new BybitCandle()
    {
        OpenTime = DateTime.UtcNow,
        OpenPrice = open,
        HighPrice = high,
        LowPrice = low,
        ClosePrice = close,
    };

    public static List<Candle> Candles(double open, double high, double low, double close) => [Candle(open, high, low, close)];
    public static List<Candle> CandlesBasic() => [Candle(100_000, 100_000, 100_000, 100_000)];
    public static TradeStrategyBase TradeStrategyShort => new TradeStrategyDummy(PositionSide.Short);
    public static TradeStrategyBase TradeStrategyLong => new TradeStrategyDummy(PositionSide.Long);
    public static BudgetStrategy BudgetStrategy => new BudgetStrategyDummy();
    public static Spot SpotShort => new(User, _budget, TradeStrategyShort, BudgetStrategy, Constants.SYMBOL_BTCUSDT, new());
    public static Spot SpotLong => new(User, _budget, TradeStrategyLong, BudgetStrategy, Constants.SYMBOL_BTCUSDT, new());

    private static User User => new("a");
}
