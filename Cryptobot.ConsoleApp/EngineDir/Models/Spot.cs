using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;
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

        var rawEntryPrice = candle.OpenPrice.Round(5);
        var slippagePerUnit = (rawEntryPrice * _slippage_multiplier).Round(5);

        var entryPrice = (position == PositionSide.Long
                ? rawEntryPrice + slippagePerUnit  // pay up when buying
                : rawEntryPrice - slippagePerUnit) // sell a bit worse when shorting
            .Round(5);

        var quantity = (tradeSize / entryPrice).Round(5);
        var entryFees = (tradeSize * Constants.TRADE_FEE).Round(5);
        var entrySlippage = (Math.Abs(entryPrice - rawEntryPrice) * quantity).Round(5);

        var budgetBeforePlaced = Budget;
        Budget = (Budget - tradeSize - entryFees).Round(5);

        var stopLossMultiplier = TradeStrategy.StopLoss(candles, currentCandleIndex, position);
        var takeProfitMultiplier = TradeStrategy.TakeProfit(candles, currentCandleIndex, position);

        double stopLoss, takeProfit;
        if (position is PositionSide.Long)
        {
            stopLoss = (entryPrice * stopLossMultiplier).Round(5);
            takeProfit = (entryPrice * takeProfitMultiplier).Round(5);
        }
        else
        {
            // mirror for short: SL above, TP below
            stopLoss = (entryPrice * (2 - stopLossMultiplier)).Round(5);      // if slMult=0.95 → 1.05
            takeProfit = (entryPrice * (2 - takeProfitMultiplier)).Round(5);  // if tpMult=1.05 → 0.95
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
            BudgetBeforePlaced = budgetBeforePlaced,
            BudgetAfterEntry = Budget,
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

            var exitFees = (trade.TradeSize * Constants.TRADE_FEE).Round(5);

            trade.IsClosed = true;
            trade.ExitTime = candle.OpenTime;
            trade.ExitCandleId = candle.Id;

            double rawExitPrice, exitPrice, exitSlip = 0;

            if (trade.PositionSide is PositionSide.Long)
            {
                if (candle.LowPrice <= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss;
                    exitPrice = (rawExitPrice * (1 - _slippage_multiplier)).Round(5);
                    exitSlip = (Math.Abs(exitPrice - rawExitPrice) * trade.Quantity).Round(5);
                }
                else if (candle.HighPrice >= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.PnL = (exitPrice * trade.Quantity) - trade.TradeSize;
            }
            else if (trade.PositionSide is PositionSide.Short)
            {
                if (candle.HighPrice >= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss;
                    exitPrice = (rawExitPrice * (1 + _slippage_multiplier)).Round(5);
                    exitSlip = (Math.Abs(exitPrice - rawExitPrice) * trade.Quantity).Round(5);
                }
                else if (candle.LowPrice <= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.PnL = trade.TradeSize - (exitPrice * trade.Quantity);
            }
            else
            {
                throw new NotImplementedException();
            }

            trade.ExitPrice = exitPrice;

            trade.SlippageCosts = (trade.SlippageCosts + exitSlip).Round(5);

            var entryFees = trade.TradeFees;
            trade.TradeFees = (trade.TradeFees + exitFees).Round(5);

            trade.PnL = (trade.PnL.Value - trade.TradeFees).Round(5);

            // Add the entry fees to avoid calculating them twice since they are already in the PnL
            Budget = (Budget + trade.TradeSize + trade.PnL.Value + entryFees).Round(5);

            trade.BudgetAfterExit = Budget;
        }
    }
}
