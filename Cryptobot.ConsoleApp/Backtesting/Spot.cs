using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Models;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting;

public class Spot
{
    private readonly double _slippage_multiplier = 0;

    public TradeStrategy TradeStrategy { get; }
    public BudgetStrategy BudgetStrategy { get; }
    public double InitialBudget { get; private set; }
    public double Budget { get; private set; }
    public List<Trade> Trades { get; } = [];

    public Spot(double budget, TradeStrategy tradeStrategy, BudgetStrategy budgetStrategy, string symbol)
    {
        TradeStrategy = tradeStrategy;
        tradeStrategy.Spot = this;

        BudgetStrategy = budgetStrategy;
        BudgetStrategy.Spot = this;

        InitialBudget = budget;
        Budget = budget;

        _slippage_multiplier = symbol switch
        {
            Constants.SYMBOL_BTCUSDT => Constants.SLIPPAGE_MULTIPLIER_BTC,
            _ => throw new NotImplementedException()
        };
    }

    public void OpenTrade<T>(List<T> candles, int currentCandleIndex) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        var tradeSize = BudgetStrategy.DefineTradeSize();

        var rawEntryPrice = candle.OpenPrice;
        var entryPrice = rawEntryPrice * (1 + _slippage_multiplier);
        var quantity = tradeSize / entryPrice;
        var slippageCosts = (entryPrice - rawEntryPrice) * quantity;
        var entryFees = tradeSize * Constants.TRADE_FEE;

        var budgetBeforePlaced = Budget;
        Budget -= tradeSize + entryFees + slippageCosts;

        Trades.Add(new Trade
        {
            EntryTime = candle.OpenTime,
            EntryPrice = entryPrice,
            EntryCandleId = candle.Id,
            StopLoss = entryPrice * TradeStrategy.StopLoss(candles, currentCandleIndex),
            TakeProfit = entryPrice * TradeStrategy.TakeProfit(candles, currentCandleIndex),
            Quantity = quantity,
            TradeSize = tradeSize,
            TradeFees = entryFees,
            SlippageCosts = slippageCosts,
            IsClosed = false,
            BudgetAfterPlaced = Budget,
            BudgetBeforePlaced = budgetBeforePlaced
        });
    }

    public void CheckCloseTrades<T>(List<T> candles, int currentCandleIndex) where T : Candle
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
                trade.ExitCandleId = candle.Id;

                double exitSlippage = 0;

                if (candle.LowPrice <= trade.StopLoss)
                {
                    var rawExitPrice = trade.StopLoss;
                    trade.ExitPrice = rawExitPrice * (1 - _slippage_multiplier);

                    exitSlippage = (rawExitPrice - trade.ExitPrice.Value) * trade.Quantity;
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

                trade.PnL = (trade.ExitPrice.Value * trade.Quantity) - trade.TradeSize - exitFees - exitSlippage;
                trade.SlippageCosts += exitSlippage;
                trade.TradeFees = exitFees;

                Budget += trade.TradeSize + trade.PnL.Value;
                trade.BudgetAfterClosed = Budget;
            }
        }
    }
}
