using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Repositories;
using Cryptobot.ConsoleApp.Resources.CachedIndicators;
using Cryptobot.ConsoleApp.Utils;
using static Cryptobot.ConsoleApp.Utils.ConsoleColors;

namespace Cryptobot.ConsoleApp.Services;

public class CacheService
{
    private readonly string _cachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), "MacroTrend.parquet");

    public Dictionary<long, CachedMacroTrend> MacroTrendCache { get; private set; } = [];

    public async Task InitializeCache()
    {
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.Fifteen_Minutes));
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.One_Day));
        await ComputeMacroTrend();
        await CandleValidatorService.ValidateStoredResources();
    }

    private async Task ComputeMacroTrend()
    {
        Printer.WriteLine($"Computing Macro Trend cache...", White);

        List<CachedMacroTrend> cachedData = [];

        if (File.Exists(_cachePath))
        {
            cachedData = await ParquetService.LoadMacroTrend(_cachePath);
            MacroTrendCache = cachedData.ToDictionary(x => x.DayTicks, x => x);

            var latestCachedTicks = cachedData.Max(x => x.DayTicks);
            var today = DateTime.UtcNow.Date;

            if (latestCachedTicks >= today.Date.Ticks)
            {
                Printer.AlreadyComputed();
                return;
            }

            Printer.WriteLine($"🟨 Cache outdated (latest: {latestCachedTicks:yyyy-MM-dd}), updating...", Cyan);
        }
        else
        {
            Printer.WriteLine("⚙️ MacroTrend cache not found, computing from scratch...", Cyan);
        }

        var candles = await CandlesRepository.GetCandles<TrendCandle>(new BacktestingDetails(CandleInterval.One_Day));

        var indicatorManager = new IndicatorService(null, [IndicatorType.MovingAverage]);
        var totalCandles = candles.Count;

        var result = cachedData.ToDictionary(x => x.DayTicks, x => x) ?? [];
        var remainingCandles = totalCandles - cachedData!.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            var candle = candles[i];

            if (result.ContainsKey(candle.DayTicks))
            {
                continue;
            }

            Printer.CalculatingCandles(i, remainingCandles);

            indicatorManager.CalculateRelevantIndicators(candles, i);
            var trend = TrendProfiler.ProfileByMovingAverage(null, candles, i);

            result[candle.DayTicks] = new CachedMacroTrend(candle.OpenTime, candle.Indicators.MovingAverage, trend);
        }

        var computed = result.Values.OrderBy(x => x.DayTicks).ToList();

        await ParquetService.SaveMacroTrend(computed, _cachePath);

        MacroTrendCache = computed.ToDictionary(x => x.DayTicks, x => x);
    }
}
