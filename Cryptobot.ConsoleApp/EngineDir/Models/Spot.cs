using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Spot
{
    private readonly double _slippage_multiplier = 0;

    public User User { get; }
    public TradeStrategy TradeStrategy { get; }
    public BudgetStrategy BudgetStrategy { get; }
    public double InitialBudget { get; private set; }
    public double Budget { get; private set; }
    public List<Trade> Trades { get; } = [];

    public Spot(User user, double budget, TradeStrategy tradeStrategy, BudgetStrategy budgetStrategy, string symbol)
    {
        User = user;
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

    public void OpenTrade<T>(List<T> candles, int currentCandleIndex, PositionSide position) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        var tradeSize = BudgetStrategy.DefineTradeSize();

        var rawEntryPrice = candle.OpenPrice;

        var entryPrice = position == PositionSide.Long
            ? rawEntryPrice * (1 + _slippage_multiplier)  // pay up when buying
            : rawEntryPrice * (1 - _slippage_multiplier); // sell a bit worse when shorting

        var quantity = tradeSize / entryPrice;
        var entryFees = tradeSize * Constants.TRADE_FEE;
        var entrySlippage = Math.Abs(entryPrice - rawEntryPrice) * quantity;

        var budgetBeforePlaced = Budget;
        Budget -= tradeSize + entryFees + entrySlippage;

        var stopLossMultiplier = TradeStrategy.StopLoss(candles, currentCandleIndex);
        var takeProfitMultiplier = TradeStrategy.TakeProfit(candles, currentCandleIndex);

        double stopLoss, takeProfit;
        if (position is PositionSide.Long)
        {
            stopLoss = entryPrice * stopLossMultiplier;
            takeProfit = entryPrice * takeProfitMultiplier;
        }
        else
        {
            // mirror for short: SL above, TP below
            stopLoss = entryPrice * (2 - stopLossMultiplier);      // if slMult=0.95 → 1.05
            takeProfit = entryPrice * (2 - takeProfitMultiplier);  // if tpMult=1.05 → 0.95
        }

        Trades.Add(new Trade
        {
            PositionSide = position,
            EntryTime = candle.OpenTime,
            EntryPrice = entryPrice,
            EntryCandleId = candle.Id,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            Quantity = quantity,
            TradeSize = tradeSize,
            TradeFees = entryFees,
            SlippageCosts = entrySlippage,
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
            if (!TradeStrategy.ShouldCloseTrade(candles, currentCandleIndex, trade))
            {
                continue;
            }

            var exitFees = trade.TradeSize * Constants.TRADE_FEE;

            trade.IsClosed = true;
            trade.ExitTime = candle.OpenTime;
            trade.ExitCandleId = candle.Id;

            double rawExitPrice, exitPrice, exitSlip = 0;

            if (trade.PositionSide is PositionSide.Long)
            {
                if (candle.LowPrice <= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss;
                    exitPrice = rawExitPrice * (1 - _slippage_multiplier);
                    exitSlip = (rawExitPrice - exitPrice) * trade.Quantity;
                }
                else if (candle.HighPrice >= trade.TakeProfit)
                {
                    rawExitPrice = trade.TakeProfit;
                    exitPrice = rawExitPrice;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.ExitPrice = exitPrice;
                trade.PnL = (exitPrice * trade.Quantity) - trade.TradeSize - exitFees - exitSlip;
            }
            else if (trade.PositionSide is PositionSide.Short)
            {
                if (candle.HighPrice >= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss;
                    exitPrice = rawExitPrice * (1 + _slippage_multiplier);
                    exitSlip = (exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (candle.LowPrice <= trade.TakeProfit)
                {
                    rawExitPrice = trade.TakeProfit;
                    exitPrice = rawExitPrice;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.ExitPrice = exitPrice;
                trade.PnL = ((trade.EntryPrice - exitPrice) * trade.Quantity) - exitFees - exitSlip;
            }
            else
            {
                throw new NotImplementedException();
            }

            trade.SlippageCosts += exitSlip;
            trade.TradeFees += exitFees;
            Budget += trade.TradeSize + trade.PnL.Value;
            trade.BudgetAfterClosed = Budget;
        }
    }
}
