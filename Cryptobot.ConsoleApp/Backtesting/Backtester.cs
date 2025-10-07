using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.ConsoleApp.Utils;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp.Backtesting;

public class Backtester(CacheManager cacheManager)
{
    private readonly CacheManager _cacheManager = cacheManager;

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
                var spot = new Spot(user, details.Budget, strategyToRun, strategy.BudgetStrategy, details.Symbol);
                Printer.BacktesterInitialization(spot);

                spot = Backtest(spot, candles);
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

    private Spot Backtest<T>(Spot spot, List<T> candles) where T : Candle
    {
        var engine = new Engine<T>(_cacheManager, spot);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            engine.TradeNewCandle(candles, i);
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
        var indicatorManager = new IndicatorManager(_cacheManager, [IndicatorType.MovingAverage]);
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

    public static async Task<List<T>> GetCandles<T>(BacktestingDetails details) where T : BybitCandle, new()
    {
        var resourcesPath = PathHelper.GetHistoryPath(details);
        var files = Directory.GetFiles(resourcesPath, $"*{Constants.PARQUET}").OrderBy(f => f);

        var allCandles = new List<T>();

        foreach (var filepath in files)
        {
            var candles = await ParquetManager.LoadCandles<T>(filepath);

            if (candles != null)
            {
                allCandles.AddRange(candles);
            }
        }

        return allCandles;
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

        var allCandles = await GetCandles<T>(details);

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

    private static async Task SaveTrendProfilerResult(List<TrendCandle> candles, BacktestingDetails details, string trendProfilerName)
    {
        var outputPath = PathHelper.GetTrendProfilingOutputPath();

        var candlesFilepath = $"{outputPath}\\{DateTime.Now:yyyy-MM-dd HH_mm}-{details.IntervalShortString}--{trendProfilerName}{Constants.PARQUET}";
        await ParquetManager.SaveTrendCandles(candles, candlesFilepath);
    }
}
