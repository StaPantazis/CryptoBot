using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Backtesting;

public static class BacktestGraphCreator
{
    public static (BybitOutputCandle[] outputCandles, double totalPnL) CreateOutputCandles(List<BybitCandle> candles, Spot spot)
    {
        var outputCandles = candles.Select(BybitOutputCandle.FromBybitCandle).ToArray();
        var candleMap = outputCandles.ToDictionary(x => x.Id);
        double totalPnL = 0;

        for (var i = 1; i < spot.Trades.Count + 1; i++)
        {
            var trade = spot.Trades[i - 1];

            var entryCandle = candleMap[trade.EntryCandleId];
            entryCandle.TradeIndex = i;
            entryCandle.EntryPrice = trade.EntryPrice.Round(1);
            entryCandle.StopLoss = trade.StopLoss?.Round(1);
            entryCandle.TakeProfit = trade.TakeProfit?.Round(1);
            entryCandle.EntryTradeSize = trade.TradeSize.Round(1);
            entryCandle.BudgetAfterEntry = trade.AvailableBudgetAfterEntry.Round(1);

            if (trade.ExitCandleId != null && candleMap.TryGetValue(trade.ExitCandleId, out var exitCandle))
            {
                exitCandle.TradeIndex = i;
                exitCandle.ExitPrice = trade.ExitPrice?.Round(1);
                entryCandle.ExitTradeSize = trade.TradeSize.Round(1);
                exitCandle.PnL = trade.PnL?.Round(2);
                exitCandle.IsProfit = trade.PnL >= 0;
                exitCandle.BudgetAfterExit = trade.AvailableBudgetAfterExit!.Value.Round(1);

                totalPnL += exitCandle.PnL!.Value.Round(1);
                exitCandle.TotalPnL = totalPnL;
            }
        }

        return (outputCandles.OrderBy(x => x.OpenTime).ToArray(), totalPnL);
    }

    public static List<LinearGraphNode> CreateLinearGraphNodes(Spot spot)
    {
        var nodes = new List<LinearGraphNode>
        {
            new(0, spot.InitialBudget, null, DateTime.ParseExact("26/03/2020", "dd/MM/yyyy", null))
        };

        var tradeIndex = 1;
        var activeTrades = new List<Trade>();

        foreach (var trade in spot.Trades.OrderBy(t => t.EntryTime))
        {
            activeTrades.Add(trade);

            var entryBudget = trade.AvailableBudgetAfterEntry;
            nodes.Add(new LinearGraphNode(tradeIndex, entryBudget, true, trade.EntryTime));

            if (trade.IsClosed && trade.ExitTime.HasValue && trade.AvailableBudgetAfterExit.HasValue)
            {
                activeTrades.Remove(trade);

                var totalBudget = trade.AvailableBudgetAfterExit.Value;

                foreach (var open in activeTrades)
                {
                    totalBudget += open.AvailableBudgetAfterEntry - open.AvailableBudgetBeforePlaced;
                }

                nodes.Add(new LinearGraphNode(tradeIndex, totalBudget, false, trade.ExitTime.Value));
            }

            tradeIndex++;
        }

        nodes.RemoveAll(x => x.IsOpen is true);

        return nodes
            .OrderBy(n => n.Timestamp)
            .ToList();
    }
}
