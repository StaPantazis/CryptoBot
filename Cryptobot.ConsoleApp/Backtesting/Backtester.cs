using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
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

public class Backtester(CacheService cacheManager)
{
    private readonly CacheService _cacheManager = cacheManager;

    public async Task RunBacktest(BacktestingDetails details)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        var candles = await GetCandlesWithPrint<BybitCandle>(details);

        foreach (var strategy in details.Strategies)
        {
            (Spot spot, Stopwatch sw) innerLoop(TradeStrategyBase strategyToRun)
            {
                var sw = new Stopwatch();
                sw.Start();

                var user = new User("Xatzias");
                var spot = new Spot(user, details.Budget, strategyToRun, strategy.BudgetStrategy, details.Symbol, _cacheManager);
                Printer.BacktesterInitialization(spot);

                spot = Backtest(spot, candles, details.Interval);
                sw.Stop();

                return (spot, sw);
            }

            if (strategy.TradeStrategy is VariationTradeStrategy<BybitCandle> variationStrategy)
            {
                var results = new List<(Spot spot, Stopwatch sw)>();
                var variations = variationStrategy.GetVariations();

                foreach (var variation in variations)
                {
                    var result = innerLoop(variation);
                    results.Add(result);

                    Printer.EmptyLine();
                    Printer.EmptyLine();
                }

                results = results
                    .OrderByDescending(x => x.spot.Trades.Count > 0)
                    .ThenByDescending(x => x.spot.InitialBudget + x.spot.Trades.Where(x => x.IsClosed).Sum(x => x.PnL ?? 0))
                    .ToList();

                Printer.VariationsResult(results.Count);
                var index = 1;

                foreach (var (spot, sw) in results)
                {
                    Printer.BacktesterStrategyName(spot, index);
                    Printer.BacktesterResult(spot, sw);
                    Printer.Divider();

                    index++;
                }
            }
            else
            {
                var (spot, sw) = innerLoop(strategy.TradeStrategy);

                Printer.BacktesterResult(spot, sw);

                var saveOutput = ShouldSaveLoop(swMain);

                if (saveOutput)
                {
                    sw.Restart();
                    Printer.SavingOutputStart();
                    await SaveBacktestResult(candles, spot);
                    Printer.SavingOutputEnd(candles.Count, sw);
                }

                Printer.Divider();
            }
        }

        if (details.Strategies.Length > 1 || details.Strategies.Any(x => x.TradeStrategy is VariationTradeStrategy<BybitCandle>))
        {
            Printer.TotalRuntime(swMain);
        }
    }

    private Spot Backtest<T>(Spot spot, List<T> candles, CandleInterval candleInterval) where T : Candle
    {
        var engine = new Engine<T>(_cacheManager, spot);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            engine.TradeNewCandle(candles, i, candleInterval);
        }

        return spot;
    }

    public async Task RunTrendProfiler(BacktestingDetails details, TrendConfiguration trendConfiguration, IndicatorType profilerScope)
    {
        var swMain = new Stopwatch();
        swMain.Start();

        Printer.ProfilerInitialization();

        var candles = await GetCandlesWithPrint<TrendCandle>(details);

        var trendProfiler = new TrendProfiler(trendConfiguration);
        var indicatorManager = new IndicatorService(_cacheManager, [IndicatorType.MovingAverage]);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);

            if (profilerScope is IndicatorType.MacroTrend)
            {
                indicatorManager.CalculateRelevantIndicators(candles, i);
            }

            var trend = profilerScope is IndicatorType.MacroTrend
                ? TrendProfiler.ProfileByMovingAverage(_cacheManager, candles, i)
                : trendProfiler.ProfileComplex(candles, i);

            candles[i].Trend = trend;
        }

        Printer.ProfilerRunEnded(candles, swMain);

        var sw = new Stopwatch();
        sw.Start();

        var saveOutput = ShouldSaveLoop(swMain);

        if (saveOutput)
        {
            Printer.SavingOutputStart();

            var profilingName = profilerScope is IndicatorType.MacroTrend ? "Macro_Trend" : trendConfiguration.ToString();
            await SaveTrendProfilerResult(candles, details, profilingName);

            Printer.SavingOutputEnd(candles.Count, sw);
        }

        Printer.EmptyLine();
        Printer.TotalRuntime(swMain);
    }

    private static bool ShouldSaveLoop(Stopwatch sw)
    {
        sw.Stop();
        bool saveOutput;

        while (true)
        {
            Printer.ShouldSaveQuestion();
            var shouldSave = Console.ReadLine();

            if ((shouldSave?.ToLower() ?? "") is "y" or "n")
            {
                saveOutput = shouldSave!.Equals("y", StringComparison.CurrentCultureIgnoreCase);
                break;
            }
        }

        sw.Start();

        return saveOutput;
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
