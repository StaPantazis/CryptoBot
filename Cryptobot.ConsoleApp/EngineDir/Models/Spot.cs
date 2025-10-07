using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Spot
{
    private readonly double _slippage_multiplier = 0;
    private readonly List<Trade> _openTrades = [];
    private readonly List<Trade> _allTrades = [];

    public string Id { get; } = Guid.NewGuid().ToString();
    public User User { get; }
    public TradeStrategyBase TradeStrategy { get; }
    public BudgetStrategy BudgetStrategy { get; }
    public double InitialBudget { get; private set; }
    public double Budget { get; private set; }
    public IReadOnlyList<Trade> Trades => _allTrades;

    public Spot(User user, double budget, TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy, string symbol)
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
        var slippagePerUnit = rawEntryPrice * _slippage_multiplier;

        var entryPrice = position == PositionSide.Long
                ? rawEntryPrice + slippagePerUnit  // pay up when buying
                : rawEntryPrice - slippagePerUnit // sell a bit worse when shorting
            ;

        var quantity = tradeSize / entryPrice;
        var entryFees = tradeSize * Constants.TRADE_FEE;
        var entrySlippage = Math.Abs(entryPrice - rawEntryPrice) * quantity;

        var budgetBeforePlaced = Budget;
        Budget = Budget - tradeSize - entryFees;

        var stopLossMultiplier = TradeStrategy.StopLoss(candles, currentCandleIndex, position);
        var takeProfitMultiplier = TradeStrategy.TakeProfit(candles, currentCandleIndex, position);

        var stopLoss = entryPrice * stopLossMultiplier;
        var takeProfit = entryPrice * takeProfitMultiplier;

        var newTrade = new Trade
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
        };

        _openTrades.Add(newTrade);
        _allTrades.Add(newTrade);
    }

    public void CheckCloseTrades<T>(List<T> candles, int currentCandleIndex) where T : Candle
    {
        if (_openTrades.Count == 0)
        {
            return;
        }

        var candle = candles[currentCandleIndex];

        for (var i = _openTrades.Count - 1; i >= 0; i--)  // backwards so we can remove easily
        {
            var trade = _openTrades[i];

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
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (candle.HighPrice >= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.PnL = (exitPrice - trade.EntryPrice) * trade.Quantity;
            }
            else if (trade.PositionSide is PositionSide.Short)
            {
                if (candle.HighPrice >= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss;
                    exitPrice = rawExitPrice * (1 + _slippage_multiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (candle.LowPrice <= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.PnL = (trade.EntryPrice - exitPrice) * trade.Quantity;
            }
            else
            {
                throw new NotImplementedException();
            }

            trade.ExitPrice = exitPrice;

            trade.SlippageCosts = trade.SlippageCosts + exitSlip;

            var entryFees = trade.TradeFees;
            trade.TradeFees = trade.TradeFees + exitFees;

            trade.PnL = trade.PnL.Value - trade.TradeFees;

            // Add the entry fees to avoid calculating them twice since they are already in the PnL
            Budget = Budget + trade.TradeSize + trade.PnL.Value + entryFees;

            trade.BudgetAfterExit = Budget;
            _openTrades.RemoveAt(i);
        }
    }
}
