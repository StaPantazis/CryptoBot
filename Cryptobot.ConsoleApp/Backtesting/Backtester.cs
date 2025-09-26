using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp.Backtesting;

public static class Backtester
{
    public static void Run(BacktestingDetails details)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        var candles = GetCandlesticks(details);

        foreach (var strategy in details.Strategies)
        {
            var sw = new Stopwatch();
            sw.Start();

            var user = new User("Xatzias");
            var spot = new Spot(user, details.Budget, strategy.TradeStrategy, strategy.BudgetStrategy, details.Symbol);
            Printer.BacktesterInitialization(spot);

            spot = Backtest(spot, candles);
            Printer.BacktesterResult(spot, sw);

            sw.Restart();
            Printer.BacktesterOutputStart();

            candles = candles.Take(1000).ToList();
            var outputCandles = SaveBacktest(candles, spot);

            Printer.BacktesterOutputEnd(outputCandles.Length, sw);
            Printer.Divider();
        }

        if (details.Strategies.Length > 1)
        {
            Printer.BacktesterTotalRuntime(swMain);
        }

        Printer.Done();
    }

    private static Spot Backtest(Spot spot, List<BybitCandle> candles)
    {
        var engine = new Engine(spot);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            engine.CheckNewCandle(candles, i);
        }

        return spot;
    }

    private static List<BybitCandle> GetCandlesticks(BacktestingDetails details)
    {
        var resourcesPath = PathHelper.GetHistoryPath(details);
        var files = Directory.GetFiles(resourcesPath, "*.json").OrderBy(f => f);

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

    private static BybitOutputCandle[] SaveBacktest(List<BybitCandle> candles, Spot spot)
    {
        var outputCandles = candles.Select(BybitOutputCandle.FromBybitCandle).ToArray();
        var candleMap = outputCandles.ToDictionary(x => x.Id);

        for (var i = 1; i < spot.Trades.Count + 1; i++)
        {
            var trade = spot.Trades[i - 1];

            var entryCandle = candleMap[trade.EntryCandleId];
            entryCandle.TradeIndex = i;
            entryCandle.EntryPrice = trade.EntryPrice.Round(1);
            entryCandle.StopLoss = trade.StopLoss.Round(1);
            entryCandle.TakeProfit = trade.TakeProfit.Round(1);
            entryCandle.TradeSize = trade.TradeSize.Round(1);

            if (trade.ExitCandleId != null && candleMap.TryGetValue(trade.ExitCandleId, out var exitCandle))
            {
                exitCandle.TradeIndex = i;
                exitCandle.ExitPrice = trade.ExitPrice?.Round(1);
                exitCandle.PnL = trade.PnL?.Round(1);
                exitCandle.IsProfit = trade.PnL >= 0;
            }
        }

        outputCandles = outputCandles.OrderBy(x => x.OpenTime).ToArray();

        var output = JsonConvert.SerializeObject(
            outputCandles,
            Formatting.None,
            new JsonSerializerSettings
            {
                ContractResolver = new ShortNameContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            });

        var outputPath = PathHelper.GetBacktestingOutputPath();
        var filename = $"{outputPath}\\{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}.json";

        ZipCompressor.CompressToGzip(output, filename);

        return outputCandles;
    }
}
