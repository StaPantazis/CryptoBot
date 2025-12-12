using Cryptobot.ConsoleApp.Backtesting.Metrics;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Spot
{
    private readonly FeesSettings _fees;
    private readonly List<Trade> _allTrades = [];

    public string Id { get; } = Guid.NewGuid().ToString();
    public User User { get; }
    public TradeStrategyBase TradeStrategy { get; }
    public BudgetStrategy BudgetStrategy { get; }
    public double InitialBudget { get; private set; }
    public double FullBudget { get; private set; }
    public double AvailableBudget { get; private set; }
    public List<Trade> OpenTrades { get; } = [];
    public IReadOnlyList<Trade> Trades => _allTrades;
    public MetricsBundle Metrics { get; private set; }

    public Spot(User user, double budget, TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy, FeesSettings fees)
    {
        User = user;
        TradeStrategy = tradeStrategy;
        tradeStrategy.Spot = this;

        BudgetStrategy = budgetStrategy;
        BudgetStrategy.Spot = this;

        InitialBudget = budget;
        FullBudget = budget;
        AvailableBudget = budget;

        _fees = fees;
    }

    public void OpenTrade<T>(CandleSlice<T> slice, PositionSide position) where T : Candle
    {
        var tradeSize = BudgetStrategy.DefineTradeSize();

        if (tradeSize <= 0 || tradeSize + (tradeSize * _fees.TradeFeeMultiplier) > AvailableBudget)
        {
            return;
        }

        var rawEntryPrice = slice.LiveCandle.OpenPrice;
        var slippagePerUnit = rawEntryPrice * _fees.SlippageMultiplier;

        var entryPrice = position == PositionSide.Long
                ? rawEntryPrice + slippagePerUnit  // pay up when buying
                : rawEntryPrice - slippagePerUnit; // sell a bit worse when shorting

        var quantity = tradeSize / entryPrice;
        var entryFees = tradeSize * _fees.TradeFeeMultiplier;
        var entrySlippage = Math.Abs(entryPrice - rawEntryPrice) * quantity;

        var availableBudgetBeforePlaced = AvailableBudget;
        AvailableBudget = AvailableBudget - tradeSize - entryFees;

        var stopLossMultiplier = TradeStrategy.StopLoss(slice, position);
        var takeProfitMultiplier = TradeStrategy.TakeProfit(slice, position);

        var stopLoss = entryPrice * stopLossMultiplier;
        var takeProfit = entryPrice * takeProfitMultiplier;

        var newTrade = new Trade
        {
            PositionSide = position,
            EntryTime = slice.LiveCandle.OpenTime,
            EntryPrice = entryPrice,
            EntryCandleId = slice.LiveCandle.Id,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            Quantity = quantity,
            TradeSize = tradeSize,
            TradeFees = entryFees,
            SlippageCosts = entrySlippage,
            IsClosed = false,
            AvailableBudgetBeforePlaced = availableBudgetBeforePlaced,
            AvailableBudgetAfterEntry = AvailableBudget,
            FullBudgetOnEntry = FullBudget,
        };

        OpenTrades.Add(newTrade);
        _allTrades.Add(newTrade);
    }

    public void CheckCloseTrades<T>(CandleSlice<T> slice) where T : Candle
    {
        if (OpenTrades.Count == 0)
        {
            return;
        }

        for (var i = OpenTrades.Count - 1; i >= 0; i--)  // backwards so we can remove easily
        {
            var trade = OpenTrades[i];

            if (!TradeStrategy.ShouldCloseTrade(slice, trade))
            {
                continue;
            }

            trade.IsClosed = true;
            trade.ExitTime = slice.LiveCandle.OpenTime;
            trade.ExitCandleId = slice.LiveCandle.Id;

            double rawExitPrice, exitPrice, exitSlip = 0;

            if (trade.PositionSide is PositionSide.Long)
            {
                if (slice.LastCandle.LowPrice <= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss!.Value;
                    exitPrice = rawExitPrice * (1 - _fees.SlippageMultiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (slice.LastCandle.HighPrice >= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit!.Value;
                }
                else if (TradeStrategy.ShouldExitLongTrade(slice, trade))
                {
                    rawExitPrice = slice.LiveCandle.OpenPrice;
                    exitPrice = rawExitPrice * (1 - _fees.SlippageMultiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else
                {
                    throw new NotImplementedException();
                }

                trade.PnL = (exitPrice - trade.EntryPrice) * trade.Quantity;
            }
            else if (trade.PositionSide is PositionSide.Short)
            {
                if (slice.LastCandle.HighPrice >= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss!.Value;
                    exitPrice = rawExitPrice * (1 + _fees.SlippageMultiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (slice.LastCandle.LowPrice <= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit!.Value;
                }
                else if (TradeStrategy.ShouldExitShortTrade(slice, trade))
                {
                    rawExitPrice = slice.LiveCandle.OpenPrice;
                    exitPrice = rawExitPrice * (1 + _fees.SlippageMultiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
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
            trade.SlippageCosts += exitSlip;

            var entryFees = trade.TradeFees;
            var exitNotional = exitPrice * trade.Quantity;
            var exitFees = exitNotional * _fees.TradeFeeMultiplier;

            trade.TradeFees += exitFees;
            trade.PnL = trade.PnL.Value - trade.TradeFees;

            // Add the entry fees to avoid calculating them twice since they are already in the PnL
            AvailableBudget = AvailableBudget + trade.TradeSize + trade.PnL.Value + entryFees;
            FullBudget += trade.PnL.Value;

            trade.AvailableBudgetAfterExit = AvailableBudget;
            trade.FullBudgetAfterExit = FullBudget;

            OpenTrades.RemoveAt(i);

            return;
        }

        return;
    }

    public void CalculateMetrics() => Metrics = new(this);
}
