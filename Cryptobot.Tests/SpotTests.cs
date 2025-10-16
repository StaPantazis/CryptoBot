using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.Tests.Mocks;
using FluentAssertions;

namespace Cryptobot.Tests;

public class SpotTests
{
    private const double _precision = 0.001;

    [Fact]
    public void Long_TakeProfit_ShouldYieldPositivePnL()
    {
        var spot = A.SpotLong;
        var candles = A.CandlesBasic();
        spot.OpenTrade(candles, 0, PositionSide.Long);
        var trade = spot.Trades[0];

        trade.IsClosed.Should().BeFalse();
        trade.EntryTime.Should().Be(candles[0].OpenTime);
        trade.EntryCandleId.Should().Be(candles[0].Id);

        trade.SlippageCosts.Should().Be(10);
        trade.EntryPrice.Should().Be(100_010);
        trade.TradeSize.Should().Be(100_000);
        trade.TradeFees.Should().Be(55);
        trade.StopLoss.Should().Be(95_009.5);
        trade.TakeProfit.Should().Be(105_010.5);
        trade.Quantity.Should().Be(0.9999);
        trade.BudgetBeforePlaced.Should().Be(1_000_000);
        trade.BudgetAfterEntry.Should().Be(899_935);

        var tpCandle = A.Candle(200_000, 250_000, 150_000, 210_000);
        candles.Add(tpCandle);

        spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);

        trade.IsClosed.Should().BeTrue();
        trade.ExitTime.Should().Be(candles.Last().OpenTime);
        trade.ExitCandleId.Should().Be(candles.Last().Id);

        trade.ExitPrice.Should().Be(trade.TakeProfit);
        trade.SlippageCosts.Should().Be(10);
        trade.TradeFees.Should().Be(110);
        trade.PnL.Should().Be(4_879.99895);
        trade.BudgetAfterExit.Should().Be(1_004_879.99895);
    }

    [Fact]
    public void Long_StopLoss_ShouldYieldNegativePnL()
    {
        var spot = A.SpotLong;
        var candles = A.CandlesBasic();
        spot.OpenTrade(candles, 0, PositionSide.Long);
        var trade = spot.Trades[0];

        trade.IsClosed.Should().BeFalse();
        trade.EntryTime.Should().Be(candles[0].OpenTime);
        trade.EntryCandleId.Should().Be(candles[0].Id);

        trade.SlippageCosts.Should().Be(10);
        trade.EntryPrice.Should().Be(100_010);
        trade.TradeSize.Should().Be(100_000);
        trade.TradeFees.Should().Be(55);
        trade.StopLoss.Should().Be(95_009.5);
        trade.TakeProfit.Should().Be(105_010.5);
        trade.Quantity.Should().Be(0.9999);
        trade.BudgetBeforePlaced.Should().Be(1_000_000);
        trade.BudgetAfterEntry.Should().Be(899_935);

        var tpCandle = A.Candle(20_000, 25_000, 15_000, 21_000);
        candles.Add(tpCandle);

        spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);

        trade.IsClosed.Should().BeTrue();
        trade.ExitTime.Should().Be(candles.Last().OpenTime);
        trade.ExitCandleId.Should().Be(candles.Last().Id);

        trade.ExitPrice.Should().Be(95_000);
        trade.SlippageCosts.Should().Be(19.5);
        trade.TradeFees.Should().Be(110);
        trade.PnL.Should().Be(-5_139);
        trade.BudgetAfterExit.Should().Be(994_861);
    }

    [Fact]
    public void Short_TakeProfit_ShouldYieldPositivePnL()
    {
        var spot = A.SpotShort;
        var candles = A.CandlesBasic();
        spot.OpenTrade(candles, 0, PositionSide.Short);
        var trade = spot.Trades[0];

        trade.IsClosed.Should().BeFalse();
        trade.EntryTime.Should().Be(candles[0].OpenTime);
        trade.EntryCandleId.Should().Be(candles[0].Id);

        trade.SlippageCosts.Should().Be(10);
        trade.EntryPrice.Should().Be(99_990);
        trade.TradeSize.Should().Be(100_000);
        trade.TradeFees.Should().Be(55);
        trade.StopLoss.Should().Be(104_989.5);
        trade.TakeProfit.Should().Be(94_990.5);
        trade.Quantity.Should().BeApproximately(1.0001, _precision);
        trade.BudgetBeforePlaced.Should().Be(1_000_000);
        trade.BudgetAfterEntry.Should().Be(899_935);

        var tpCandle = A.Candle(20_000, 25_000, 15_000, 21_000);
        candles.Add(tpCandle);

        spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);

        trade.IsClosed.Should().BeTrue();
        trade.ExitTime.Should().Be(candles.Last().OpenTime);
        trade.ExitCandleId.Should().Be(candles.Last().Id);

        trade.ExitPrice.Should().Be(trade.TakeProfit);
        trade.SlippageCosts.Should().Be(10);
        trade.TradeFees.Should().Be(110);
        trade.PnL.Should().Be(4_880.00095);
        trade.BudgetAfterExit.Should().Be(1_004_880.00095);
    }

    [Fact]
    public void Short_StopLoss_ShouldYieldNegativePnL()
    {
        var spot = A.SpotShort;
        var candles = A.CandlesBasic();
        spot.OpenTrade(candles, 0, PositionSide.Short);
        var trade = spot.Trades[0];

        trade.IsClosed.Should().BeFalse();
        trade.EntryTime.Should().Be(candles[0].OpenTime);
        trade.EntryCandleId.Should().Be(candles[0].Id);

        trade.SlippageCosts.Should().Be(10);
        trade.EntryPrice.Should().Be(99_990);
        trade.TradeSize.Should().Be(100_000);
        trade.TradeFees.Should().Be(55);
        trade.StopLoss.Should().Be(104_989.5);
        trade.TakeProfit.Should().Be(94_990.5);
        trade.Quantity.Should().BeApproximately(1.0001, _precision);
        trade.BudgetBeforePlaced.Should().Be(1_000_000);
        trade.BudgetAfterEntry.Should().Be(899_935);

        var tpCandle = A.Candle(200_000, 250_000, 150_000, 210_000);
        candles.Add(tpCandle);

        spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);

        trade.IsClosed.Should().BeTrue();
        trade.ExitTime.Should().Be(candles.Last().OpenTime);
        trade.ExitCandleId.Should().Be(candles.Last().Id);

        trade.ExitPrice.Should().Be(105_010.49889);
        trade.SlippageCosts.Should().BeApproximately(20.49994, _precision);
        trade.TradeFees.Should().Be(110);
        trade.PnL.Should().Be(-5130.4979);
        trade.BudgetAfterExit.Should().Be(1_004_880.00095);
    }

    [Fact]
    public void Long_NoHit_ShouldThrow()
    {
        var spot = A.SpotLong;
        var candles = A.Candles(100, 100, 100, 100);
        spot.OpenTrade(candles, 0, PositionSide.Long);

        var neutral = A.Candle(100, 102, 99, 101);
        candles.Add(neutral);

        var act = () => spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Short_NoHit_ShouldThrow()
    {
        var spot = A.SpotLong;
        var candles = A.Candles(100, 100, 100, 100);
        spot.OpenTrade(candles, 0, PositionSide.Short);

        var neutral = A.Candle(100, 102, 99, 101);
        candles.Add(neutral);

        var act = () => spot.CheckCloseTrades(candles, 1, CandleInterval.One_Day);
        act.Should().Throw<NotImplementedException>();
    }
}
