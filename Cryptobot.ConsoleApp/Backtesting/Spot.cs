using Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting;

public class Spot
{
    public TradeStrategy TradeStrategy { get; }
    public BudgetStrategy BudgetStrategy { get; }
    public double Budget { get; private set; }
    public List<Trade> Trades { get; } = [];

    public Spot(double budget, TradeStrategy tradeStrategy, BudgetStrategy budgetStrategy)
    {
        TradeStrategy = tradeStrategy;
        tradeStrategy.Spot = this;

        BudgetStrategy = budgetStrategy;
        BudgetStrategy.Spot = this;

        Budget = budget;
    }

    public void OpenTrade(BybitCandle candle)
    {
        var tradeSize = BudgetStrategy.DefineTradeSize();

        var entryPrice = candle.OpenPrice;
        var quantity = tradeSize / entryPrice;
        var entryFees = tradeSize * Constants.TRADE_FEE;

        Budget -= tradeSize + entryFees;

        Trades.Add(new Trade
        {
            EntryTime = candle.OpenTime,
            EntryPrice = entryPrice,
            StopLoss = entryPrice * 0.95,
            TakeProfit = entryPrice * 1.05,
            Quantity = quantity,
            TradeSize = tradeSize,
            TradeFees = entryFees,
            IsClosed = false,
            BudgetAfterPlaced = Budget,
            BudgetBeforePlaced = Budget + tradeSize
        });
    }

    public void CheckCloseTrades(List<BybitCandle> candles, int currentCandleIndex)
    {
        var openTrades = Trades.Where(t => !t.IsClosed).ToArray();

        if (openTrades.Length == 0)
        {
            return;
        }

        var candle = candles[currentCandleIndex];

        foreach (var trade in openTrades)
        {
            if (TradeStrategy.ShouldCloseTrade(candles, currentCandleIndex, trade))
            {
                var exitFees = trade.TradeSize * Constants.TRADE_FEE;

                trade.IsClosed = true;
                trade.ExitTime = candle.OpenTime;

                if (candle.LowPrice <= trade.StopLoss)
                {
                    trade.ExitPrice = trade.StopLoss;
                }
                else if (candle.HighPrice >= trade.TakeProfit)
                {
                    trade.ExitPrice = trade.TakeProfit;
                }
                else
                {
                    // Custom implementation, don't know what to do there, let's throw for now
                    throw new NotImplementedException();
                }

                trade.PnL = (trade.ExitPrice!.Value * trade.Quantity) - trade.TradeSize - exitFees;
                trade.TradeFees = exitFees;

                Budget += trade.TradeSize + trade.PnL.Value;
                trade.BudgetAfterClosed = Budget;
            }
        }
    }
}
