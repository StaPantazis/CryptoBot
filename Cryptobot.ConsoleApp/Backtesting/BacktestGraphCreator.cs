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
            entryCandle.BudgetAfterEntry = trade.BudgetAfterEntry.Round(1);

            if (trade.ExitCandleId != null && candleMap.TryGetValue(trade.ExitCandleId, out var exitCandle))
            {
                exitCandle.TradeIndex = i;
                exitCandle.ExitPrice = trade.ExitPrice?.Round(1);
                entryCandle.ExitTradeSize = trade.TradeSize.Round(1);
                exitCandle.PnL = trade.PnL?.Round(2);
                exitCandle.IsProfit = trade.PnL >= 0;
                exitCandle.BudgetAfterExit = trade.BudgetAfterExit!.Value.Round(1);

                totalPnL += exitCandle.PnL!.Value.Round(1);
                exitCandle.TotalPnL = totalPnL;
            }
        }

        return (outputCandles.OrderBy(x => x.OpenTime).ToArray(), totalPnL);
    }

    public static List<LinearGraphNode> CreateLinearGraphNodes(BybitOutputCandle[] outputCandles, Spot spot)
    {
        var linearGraphNodes = new List<LinearGraphNode>() { new(0, 0, spot.InitialBudget, true) };
        var tradedCandles = outputCandles.Where(x => x.EntryPrice != null || x.ExitPrice != null).ToArray();

        for (var i = 0; i < tradedCandles.Length; i++)
        {
            var candle = tradedCandles[i];

            if (candle.ExitPrice != null)
            {
                linearGraphNodes.Add(new(i + 1, candle.PnL, candle.BudgetAfterExit!.Value, candle.PnL!.Value >= 0));
            }

            if (candle.EntryPrice != null)
            {
                linearGraphNodes.Add(new(i + 1, null, candle.BudgetAfterEntry!.Value, null));
            }
        }

        return linearGraphNodes;
    }

    public static List<LinearTimeGraphNode> CreateLinearTimeGraphNodes(Spot spot)
    {
        var nodes = new List<LinearTimeGraphNode>() { new(0, spot.InitialBudget, null, DateTime.ParseExact("26/03/2020", "dd/MM/yyyy", null)) };
        var tradeIndex = 1;

        foreach (var trade in spot.Trades)
        {
            var openNode = new LinearTimeGraphNode(tradeIndex, trade.BudgetAfterEntry, IsOpen: true, trade.EntryTime);
            nodes.Add(openNode);

            if (trade.IsClosed && trade.ExitTime.HasValue && trade.BudgetAfterExit.HasValue)
            {
                var closeNode = new LinearTimeGraphNode(tradeIndex, trade.BudgetAfterExit.Value, IsOpen: false, trade.ExitTime.Value);
                nodes.Add(closeNode);
            }

            tradeIndex++;
        }

        var orderedNodes = nodes
            .OrderBy(n => n.Timestamp)
            .ThenByDescending(n => n.IsOpen == true)
            .ToList();

        return orderedNodes;
    }
}
