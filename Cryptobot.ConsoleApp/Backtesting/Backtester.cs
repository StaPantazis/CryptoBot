using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.ConsoleApp.Repositories;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp.Backtesting;

public class Backtester(CacheService cache)
{
    private readonly CacheService _cache = cache;

    public async Task RunBacktest(BacktestingDetails details)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        var candles = await GetCandlesWithPrint<BybitCandle>(details);
        var spots = new List<(Spot spot, Stopwatch sw)>();
        var totalStrategies = details.Strategies.Length;

        for (var i = 0; i < totalStrategies; i++)
        {
            var strategy = details.Strategies[i];
            var sw = new Stopwatch();
            sw.Start();

            var user = new User("Xatzias");
            var spot = new Spot(user, details.Budget, strategy.TradeStrategy, strategy.BudgetStrategy, new FeesSettings());
            spots.Add((spot, sw));
            Printer.BacktesterInitialization(spot, totalStrategies > 1 ? i + 1 : null, totalStrategies > 1 ? totalStrategies : null);

            spot = Backtest(spot, candles, strategy.TradeStrategy);
            spot.CalculateMetrics();
            sw.Stop();

            if (details.Strategies.Length > 1)
            {
                Printer.FullStrategyPnL(spot);
                Printer.Divider();
            }
        }

        var totalBestStrategies = Math.Min(5, spots.Count);

        if (details.Strategies[0].IsVariationBundle || spots.Count > 50)
        {
            var bestOverallSpots = spots
                .OrderByDescending(x => x.spot.Metrics.Full.PnL)
                .Take(totalBestStrategies)
                .ToList();

            await PrintResult(bestOverallSpots, candles, totalStrategies, swMain, "Overall");
            Printer.EmptyLine();

            var bestLongSpots = spots
                .OrderByDescending(x => x.spot.Metrics.Long.PnL)
                .Take(totalBestStrategies)
                .ToList();

            await PrintResult(bestLongSpots, candles, totalStrategies, swMain, "Long");
            Printer.EmptyLine();

            var bestShortSpots = spots
                .OrderByDescending(x => x.spot.Metrics.Short.PnL)
                .Take(totalBestStrategies)
                .ToList();

            await PrintResult(bestShortSpots, candles, totalStrategies, swMain, "Short");
        }
        else
        {
            await PrintResult(spots, candles, totalStrategies, swMain);
        }

        if (details.Strategies.Length > 1)
        {
            Printer.TotalRuntime(swMain);
        }
    }

    private Spot Backtest<T>(Spot spot, List<T> candles, TradeStrategyBase tradeStrategy) where T : Candle
    {
        var engine = new Engine<T>(_cache, spot);
        var slice = new CandleSlice<T>(tradeStrategy.SliceSize);

        foreach (var (_, i) in candles.AsSeederWithSlice(slice))
        {
            Printer.CalculatingCandles(i, candles.Count);
            engine.TradeLive(slice);
        }

        return spot;
    }

    public static async Task RunTrendProfiler(BacktestingDetails details, AiTrendConfiguration? aiTrendConfig, IndicatorType indicatorType)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        Printer.ProfilerInitialization();

        var candles = await GetCandlesWithPrint<TrendCandle>(details);
        var indicatorService = new IndicatorService(aiTrendConfig, indicatorType);
        var slice = new CandleSlice<TrendCandle>(indicatorService.SliceSizeBasedOnIndicators);

        foreach (var (_, i) in candles.AsSeederWithSlice(slice))
        {
            Printer.CalculatingCandles(i, candles.Count);
            indicatorService.CalculateRelevantIndicators(slice);
        }

        Printer.ProfilerRunEnded(candles, swMain);

        var sw = new Stopwatch();
        sw.Start();

        var (shouldSave, _, _) = ShouldSaveLoop(swMain);

        if (shouldSave)
        {
            Printer.SavingOutputStart();

            var profilingName = $"{indicatorType}{(indicatorType is IndicatorType.AiTrend ? aiTrendConfig!.ToString() : "")}";
            await SaveTrendProfilerResult(candles, details, profilingName);

            Printer.SavingOutputEnd(candles.Count, sw);
        }

        Printer.EmptyLine();
        Printer.TotalRuntime(swMain);
    }

    private static async Task PrintResult(List<(Spot spot, Stopwatch sw)> spots, List<BybitCandle> candles, int totalStrategies, Stopwatch swMain, string sectionName = "")
    {
        for (var i = 0; i < spots.Count; i++)
        {
            var (spot, sw) = spots[i];
            Printer.BacktesterStrategyName(spot, totalStrategies > 1 ? i + 1 : null, totalStrategies > 1 ? totalStrategies : null);
            Printer.BacktesterResult(spot, sw, sectionName);

            if (totalStrategies == 1)
            {
                var (shouldSave, isGraph, isCsv) = ShouldSaveLoop(swMain);

                if (shouldSave)
                {
                    sw.Restart();

                    if (isCsv)
                    {
                        Printer.ExportingCsvStart();
                        CsvService.ExportTrades(spot.Trades);
                        Printer.Done();
                    }

                    if (isGraph)
                    {
                        Printer.SavingOutputStart();
                        await SaveBacktestResult(candles, spot);
                        Printer.SavingOutputEnd(candles.Count, sw);
                    }
                }
            }
            else
            {
                Printer.Divider();
            }
        }
    }

    private static (bool shouldSave, bool isGraph, bool isCsv) ShouldSaveLoop(Stopwatch sw)
    {
        sw.Stop();
        bool saveOutput, isCsv, isGraph;

        while (true)
        {
            Printer.ShouldSaveQuestion();
            var answer = Console.ReadLine();

            if ((answer?.ToLower() ?? "") is "y" or "n" or "csv" or "both")
            {
                saveOutput = answer!.Equals("y", StringComparison.CurrentCultureIgnoreCase);
                isGraph = answer.Equals("y", StringComparison.CurrentCultureIgnoreCase) || answer.Equals("graph", StringComparison.CurrentCultureIgnoreCase);
                isCsv = answer.Equals("csv", StringComparison.CurrentCultureIgnoreCase);
                break;
            }
        }

        sw.Start();

        return (saveOutput, isGraph, isCsv);
    }

    private static async Task<List<T>> GetCandlesWithPrint<T>(BacktestingDetails details) where T : BybitCandle, new()
    {
        var swLoading = new Stopwatch();
        Printer.LoadingCandlesStart(details);

        var allCandles = await CandlesRepository.GetCandles<T>(details);

        Printer.LoadingCandlesEnd(swLoading);

        return allCandles;
    }

    private static async Task SaveBacktestResult(List<BybitCandle> candles, Spot spot)
    {
        var outputPath = PathHelper.GetBacktestingOutputPath();
        var (outputCandles, totalPnL) = BacktestGraphCreator.CreateOutputCandles(candles, spot);
        var linearGraphNodes = BacktestGraphCreator.CreateLinearGraphNodes(spot);

        var candlesFilepath = $"{outputPath}\\{DateNow()}candles-{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}{Constants.PARQUET}";
        await ParquetService.SaveBacktestCandlesAsync(outputCandles, candlesFilepath);

        var linearGraphFilepath = $"{PathHelper.GetBacktestingOutputPath()}\\{DateNow()}linear-{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}{Constants.PARQUET}";
        await ParquetService.SaveLinearGraph(linearGraphNodes, linearGraphFilepath);

        var zipFilepath = $"{PathHelper.GetBacktestingOutputPath()}\\{DateNow()}{spot.TradeStrategy.NameOf}--{spot.BudgetStrategy.NameOf}__{totalPnL.Euro(digits: 1, plusIfPositive: true)}{Constants.ZIP}";
        ZipHelper.BundleFiles(zipFilepath, candlesFilepath, linearGraphFilepath);

        Printer.SavedOutputFileName(zipFilepath);

        File.Delete(candlesFilepath);
        File.Delete(linearGraphFilepath);
    }

    private static async Task SaveTrendProfilerResult(List<TrendCandle> candles, BacktestingDetails details, string trendProfilerName)
    {
        var outputPath = PathHelper.GetTrendProfilingOutputPath();

        var candlesFilepath = $"{outputPath}\\{DateNow()}{details.IntervalShortString}--{trendProfilerName}{Constants.PARQUET}";
        await ParquetService.SaveTrendCandles(candles, candlesFilepath);
    }

    private static string DateNow() => $"{DateTime.Now:yyyy-MM-dd HH_mm}__";
}
