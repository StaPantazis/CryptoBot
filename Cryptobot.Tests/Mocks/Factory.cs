using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Utils;
using Cryptobot.Tests.Mocks.Dummies;

namespace Cryptobot.Tests.Mocks;

public static class Factory
{
    public static List<Candle> Candles => [];
    public static TradeStrategy TradeStrategy => new TradeStrategyDummy();
    public static BudgetStrategy BudgetStrategy => new BudgetStrategyDummy();
    public static Spot Spot => new(new User("a"), 1000, TradeStrategy, BudgetStrategy, Constants.SYMBOL_BTCUSDT);
}
