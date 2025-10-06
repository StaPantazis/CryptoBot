using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.ConsoleApp.Utils;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp.Backtesting;

public static class Backtester
{
    public static async Task RunBacktest(BacktestingDetails details)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        var candles = await GetCandles<BybitCandle>(details);

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
            Printer.SavingOutputStart();

            await SaveBacktestResult(candles, spot);

            Printer.SavingOutputEnd(candles.Count, sw);
            Printer.Divider();
        }

        if (details.Strategies.Length > 1)
        {
            Printer.TotalRuntime(swMain);
        }
    }

    private static Spot Backtest(Spot spot, List<BybitCandle> candles)
    {
        var engine = new Engine(spot);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            engine.TradeNewCandle(candles, i);
        }

        return spot;
    }

    public static async Task RunTrendProfiler(BacktestingDetails details, TrendConfiguration trendConfiguration)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        Printer.ProfilerInitialization();

        var candles = await GetCandles<TrendCandle>(details);

        var trendProfiler = new TrendProfiler(trendConfiguration);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            var trend = trendProfiler.Profile(candles, i);

            candles[i].Trend = trend;
        }

        Printer.ProfilerRunEnded(candles, swMain);

        var sw = new Stopwatch();
        sw.Start();

        Printer.SavingOutputStart();
        await SaveTrendProfilerResult(candles, details, trendConfiguration);
        Printer.SavingOutputEnd(candles.Count, sw);

        Printer.EmptyLine();
        Printer.TotalRuntime(swMain);
    }

    private static async Task<List<T>> GetCandles<T>(BacktestingDetails details) where T : BybitCandle, new()
    {
        var swLoading = new Stopwatch();
        Printer.LoadingCandlesStart(details);

        var resourcesPath = PathHelper.GetHistoryPath(details);
        var files = Directory.GetFiles(resourcesPath, $"*{Constants.PARQUET}").OrderBy(f => f);

        var allCandles = new List<T>();

        foreach (var filepath in files)
        {
            var candles = await ParquetManager.LoadCandlesAsync<T>(filepath);

            if (candles != null)
            {
                allCandles.AddRange(candles);
            }
        }

        Printer.LoadingCandlesEnd(swLoading);

        return allCandles;
    }

    private static async Task SaveBacktestResult(List<BybitCandle> candles, Spot spot)
    {
        var outputPath = PathHelper.GetBacktestingOutputPath();

        // Candles
        var outputCandles = candles.Select(BybitOutputCandle.FromBybitCandle).ToArray();
        var candleMap = outputCandles.ToDictionary(x => x.Id);
        double totalPnL = 0;

        for (var i = 1; i < spot.Trades.Count + 1; i++)
        {
            var trade = spot.Trades[i - 1];

            var entryCandle = candleMap[trade.EntryCandleId];
            entryCandle.TradeIndex = i;
            entryCandle.EntryPrice = trade.EntryPrice.Round(1);
            entryCandle.StopLoss = trade.StopLoss.Round(1);
            entryCandle.TakeProfit = trade.TakeProfit.Round(1);
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

        outputCandles = outputCandles.OrderBy(x => x.OpenTime).ToArray();

        // Linear Graph
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

        var candlesFilepath = $"{outputPath}\\candles-{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}{Constants.PARQUET}";
        await ParquetManager.SaveBacktestCandlesAsync(outputCandles, candlesFilepath);

        var linearFilepath = $"{PathHelper.GetBacktestingOutputPath()}\\linear-{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}{Constants.PARQUET}";
        await ParquetManager.SaveLinearGraph(linearGraphNodes, linearFilepath);

        var zipFilepath = $"{PathHelper.GetBacktestingOutputPath()}\\{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}__{totalPnL.Euro(digits: 1, plusIfPositive: true)}{Constants.ZIP}";
        ZipHelper.BundleFiles(zipFilepath, candlesFilepath, linearFilepath);

        File.Delete(candlesFilepath);
        File.Delete(linearFilepath);
    }

    private static async Task SaveTrendProfilerResult(List<TrendCandle> candles, BacktestingDetails details, TrendConfiguration config)
    {
        var outputPath = PathHelper.GetTrendProfilingOutputPath();

        var candlesFilepath = $"{outputPath}\\candles-{details.IntervalShortString}--{config}{Constants.PARQUET}";
        await ParquetManager.SaveTrendCandlesAsync(candles, candlesFilepath);
    }
}
