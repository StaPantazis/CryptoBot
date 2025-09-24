using Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp.Backtesting;

public static class Backtester
{
    public static async Task Test(HistoryRequest historyRequest)
    {
        var sw = new Stopwatch();
        sw.Start();

        var outputPath = PathHelper.GetBacktestingOutputPath();
        var candles = GetCandlesticks(historyRequest);

        var spot = Run(candles);

        Printer.BacktesterResult(spot, sw);
        sw.Stop();

        var outputCandles = candles.Select(BybitOutputCandle.FromBybitCandle).ToArray();

        for (var i = 1; i < spot.Trades.Count + 1; i++)
        {
            var trade = spot.Trades[i];

            var entryCandle = outputCandles.Single(x => x.Id == trade.EntryCandleId);
            var exitCandle = outputCandles.SingleOrDefault(x => x.Id == trade.ExitCandleId);

            entryCandle.TradeIndex = i;
            entryCandle.EntryPrice = trade.EntryPrice;
            entryCandle.StopLoss = trade.StopLoss;
            entryCandle.TakeProfit = trade.TakeProfit;
            entryCandle.TradeSize = trade.TradeSize;

            if (exitCandle != null)
            {
                exitCandle.TradeIndex = i;
                exitCandle.ExitPrice = trade.ExitPrice;
                exitCandle.PnL = trade.PnL;
                exitCandle.IsProfit = trade.PnL >= 0;
            }
        }

        var output = JsonConvert.SerializeObject(outputCandles, Formatting.None);
        var filename = $"{outputPath}\\sandbox.json";

        await File.WriteAllTextAsync(filename, output);
        Printer.Done();
    }

    public static Spot Run(List<BybitCandle> candles)
    {
        var spot = new Spot(10000, new TradeStrategy_EveryFiveCandles(), new BudgetStrategy_OnePercent());
        Printer.BacktesterStrategies(spot);

        for (var i = 0; i < candles.Count; i++)
        {
            var candle = candles[i];

            if (spot.TradeStrategy.ShouldOpenTrade(candles, i))
            {
                spot.OpenTrade(candle);
            }

            spot.CheckCloseTrades(candles, i);
        }

        return spot;
    }

    private static List<BybitCandle> GetCandlesticks(HistoryRequest historyRequest)
    {
        var resourcesPath = PathHelper.GetHistoryPath(historyRequest);
        var files = Directory.GetFiles(resourcesPath, "*.json").OrderBy(f => f).Take(3);

        var allCandles = new List<BybitCandle>();

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var candles = JsonConvert.DeserializeObject<List<BybitCandle>>(json);

            if (candles != null)
            {
                allCandles.AddRange(candles);
            }
        }

        return allCandles;
    }
}
