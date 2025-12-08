using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;
using Cryptobot.ConsoleApp.Repositories;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;
using System.Diagnostics;
using System.Text;

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

        for (var i = 0; i < details.Strategies.Length; i++)
        {
            var strategy = details.Strategies[i];
            var sw = new Stopwatch();
            sw.Start();

            var user = new User("Xatzias");
            var spot = new Spot(user, details.Budget, strategy.TradeStrategy, strategy.BudgetStrategy, details.Symbol);
            spots.Add((spot, sw));
            Printer.BacktesterInitialization(spot, i + 1);

            spot = Backtest(spot, candles, details.Interval);
            spot.CalculateMetrics();
            sw.Stop();

            if (details.Strategies.Length > 1)
            {
                Printer.FullStrategyPnL(spot);
                Printer.Divider();
            }
        }

        spots = spots.OrderByDescending(x => x.spot.Metrics.Full.PnL).ToList();

        foreach (var (spot, sw) in spots)
        {
            Printer.BacktesterResult(spot, sw);

            if (details.Strategies.Length == 1)
            {
                var saveOutput = ShouldSaveLoop(swMain);

                if (saveOutput)
                {
                    sw.Restart();
                    Printer.SavingOutputStart();
                    await SaveBacktestResult(candles, spot);
                    Printer.SavingOutputEnd(candles.Count, sw);
                }
            }
            else
            {
                Printer.Divider();
            }
        }

        if (details.Strategies.Length > 1)
        {
            Printer.TotalRuntime(swMain);
        }
    }

    private Spot Backtest<T>(Spot spot, List<T> candles, CandleInterval candleInterval) where T : Candle
    {
        var engine = new Engine<T>(_cache, spot);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            engine.TradeNewCandle(candles, i, candleInterval);
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
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);
            indicatorService.CalculateRelevantIndicators(candles, i);
        }

        Printer.ProfilerRunEnded(candles, swMain);

        var sw = new Stopwatch();
        sw.Start();

        var saveOutput = ShouldSaveLoop(swMain);

        if (saveOutput)
        {
            Printer.SavingOutputStart();

            var profilingName = $"{indicatorType}{(indicatorType is IndicatorType.AiTrend ? aiTrendConfig!.ToString() : "")}";
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

    private static void QuickExportToCSV(IReadOnlyList<Trade> trades)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",",
            nameof(Trade.PositionSide),
            nameof(Trade.EntryTime),
            nameof(Trade.ExitTime),
            nameof(Trade.EntryPrice),
            nameof(Trade.ExitPrice),
            nameof(Trade.PnL),
            nameof(Trade.StopLoss),
            nameof(Trade.TakeProfit),
            nameof(Trade.Quantity),
            nameof(Trade.TradeSize),
            nameof(Trade.TradeFees),
            nameof(Trade.SlippageCosts),
            nameof(Trade.AvailableBudgetBeforePlaced),
            nameof(Trade.AvailableBudgetAfterEntry),
            nameof(Trade.AvailableBudgetAfterExit),
            nameof(Trade.FullBudgetOnEntry),
            nameof(Trade.FullBudgetAfterExit)
        ));

        // Rows
        foreach (var t in trades)
        {
            sb.AppendLine(string.Join(",",
                t.PositionSide,
                t.EntryTime.ToString("dd/MM/yyyy HH:mm"),
                t.ExitTime?.ToString("dd/MM/yyyy HH:mm") ?? "",
                t.EntryPrice,
                t.ExitPrice,
                t.PnL,
                t.StopLoss,
                t.TakeProfit,
                t.Quantity,
                t.TradeSize,
                t.TradeFees,
                t.SlippageCosts,
                t.AvailableBudgetBeforePlaced,
                t.AvailableBudgetAfterEntry,
                t.AvailableBudgetAfterExit,
                t.FullBudgetOnEntry,
                t.FullBudgetAfterExit
            ));
        }

        File.WriteAllText(@"C:\Users\stath\Downloads\trades.xlsx", sb.ToString());
    }
}
