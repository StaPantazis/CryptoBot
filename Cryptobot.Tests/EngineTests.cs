using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.Tests.Builders;

namespace Cryptobot.Tests;

public class EngineTests
{
    [Fact]
    public void Should_open_and_exit_in_the_correct_candles()
    {
        var tradeStrategy = A.TradeStrategy().WithAllowLong().Build();
        var spot = A.Spot(tradeStrategy, new BS_100Percent());
        var engine = new Engine<Candle>(A.Cache(), spot);

        var candles = A.Candles(
            A.Candle(open: 100, high: 100, low: 100, close: 100, id: 1),
            A.Candle(100, 100, 100, 100, 2),
            A.Candle(100, 150, 100, 110, 3),
            A.Candle(110, 110, 110, 110, 4),
            A.Candle(110, 110, 110, 110, 5));

        var slice = A.Slice();

        foreach (var (_, _) in candles.AsSeederWithSlice(slice))
        {
            engine.TradeLive(slice);
        }

        //spot.Trades.Should().HaveCount();
    }
}
