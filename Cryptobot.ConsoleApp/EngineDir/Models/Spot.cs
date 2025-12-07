using Cryptobot.ConsoleApp.Backtesting.Metrics;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Spot
{
    private readonly double _slippage_multiplier = 0;
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

    public Spot(User user, double budget, TradeStrategyBase tradeStrategy, BudgetStrategy budgetStrategy, string symbol)
    {
        User = user;
        TradeStrategy = tradeStrategy;
        tradeStrategy.Spot = this;

        BudgetStrategy = budgetStrategy;
        BudgetStrategy.Spot = this;

        InitialBudget = budget;
        FullBudget = budget;
        AvailableBudget = budget;

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

        if (tradeSize <= 0 || tradeSize + (tradeSize * Constants.TRADE_FEE) > AvailableBudget)
        {
            return;
        }

        var rawEntryPrice = candle.OpenPrice;
        var slippagePerUnit = rawEntryPrice * _slippage_multiplier;

        var entryPrice = position == PositionSide.Long
                ? rawEntryPrice + slippagePerUnit  // pay up when buying
                : rawEntryPrice - slippagePerUnit; // sell a bit worse when shorting

        var quantity = tradeSize / entryPrice;
        var entryFees = tradeSize * Constants.TRADE_FEE;
        var entrySlippage = Math.Abs(entryPrice - rawEntryPrice) * quantity;

        var availableBudgetBeforePlaced = AvailableBudget;
        AvailableBudget = AvailableBudget - tradeSize - entryFees;

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
            AvailableBudgetBeforePlaced = availableBudgetBeforePlaced,
            AvailableBudgetAfterEntry = AvailableBudget,
            FullBudgetOnEntry = FullBudget,
        };

        OpenTrades.Add(newTrade);
        _allTrades.Add(newTrade);
    }

    public void CheckCloseTrades<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval) where T : Candle
    {
        if (OpenTrades.Count == 0)
        {
            return;
        }

        var candle = candles[currentCandleIndex];

        for (var i = OpenTrades.Count - 1; i >= 0; i--)  // backwards so we can remove easily
        {
            var trade = OpenTrades[i];

            if (!TradeStrategy.ShouldCloseTrade(candles, currentCandleIndex, candleInterval, trade))
            {
                continue;
            }

            trade.IsClosed = true;
            trade.ExitTime = candle.OpenTime;
            trade.ExitCandleId = candle.Id;

            double rawExitPrice, exitPrice, exitSlip = 0;

            if (trade.PositionSide is PositionSide.Long)
            {
                if (candle.LowPrice <= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss!.Value;
                    exitPrice = rawExitPrice * (1 - _slippage_multiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (candle.HighPrice >= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit!.Value;
                }
                else if (TradeStrategy.ShouldExitLongTrade(candles, currentCandleIndex, trade, candleInterval))
                {
                    rawExitPrice = candles[currentCandleIndex].ClosePrice;
                    exitPrice = rawExitPrice * (1 - _slippage_multiplier);
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
                if (candle.HighPrice >= trade.StopLoss)
                {
                    rawExitPrice = trade.StopLoss!.Value;
                    exitPrice = rawExitPrice * (1 + _slippage_multiplier);
                    exitSlip = Math.Abs(exitPrice - rawExitPrice) * trade.Quantity;
                }
                else if (candle.LowPrice <= trade.TakeProfit)
                {
                    exitPrice = trade.TakeProfit!.Value;
                }
                else if (TradeStrategy.ShouldExitShortTrade(candles, currentCandleIndex, trade, candleInterval))
                {
                    rawExitPrice = candles[currentCandleIndex].ClosePrice;
                    exitPrice = rawExitPrice * (1 + _slippage_multiplier);
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
            var exitFees = exitNotional * Constants.TRADE_FEE;

            trade.TradeFees += exitFees;
            trade.PnL = trade.PnL.Value - trade.TradeFees;

            // Add the entry fees to avoid calculating them twice since they are already in the PnL
            AvailableBudget = AvailableBudget + trade.TradeSize + trade.PnL.Value + entryFees;
            FullBudget += trade.PnL.Value;

            trade.AvailableBudgetAfterExit = AvailableBudget;
            trade.FullBudgetAfterExit = FullBudget;

            OpenTrades.RemoveAt(i);
        }
    }

    public void CalculateMetrics() => Metrics = new(this);
}
