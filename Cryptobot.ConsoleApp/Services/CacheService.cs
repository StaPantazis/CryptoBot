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
    public Dictionary<long, CachedTrendProfileAI_Aggressive> SemiTrendCache { get; private set; } = [];
    public Dictionary<long, CachedMacroTrend> MacroTrendCache { get; private set; } = [];

    public async Task InitializeCache()
    {
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.Five_Minutes));
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.Fifteen_Minutes));
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.One_Day));
        await ComputeMacroTrend();
        //await ComputeSemiTrend();
        await CandleValidatorService.ValidateStoredResources();
    }

    private async Task ComputeMacroTrend()
    {
        var macroCachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), "MacroTrend.parquet");
        var (shouldSave, data) = await ComputeTrend<CachedMacroTrend>(macroCachePath, CandleInterval.One_Day);

        if (shouldSave)
        {
            await ParquetService.SaveMacroTrend(data, macroCachePath);
        }

        MacroTrendCache = data.ToDictionary(x => x.Key, x => x);
    }

    private async Task ComputeSemiTrend()
    {
        var semiCachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), "SemiTrend.parquet");
        var (shouldSave, data) = await ComputeTrend<CachedTrendProfileAI_Aggressive>(semiCachePath, CandleInterval.Fifteen_Minutes);

        if (shouldSave)
        {
            await ParquetService.SaveTrend(data, semiCachePath);
        }

        SemiTrendCache = data.ToDictionary(x => x.Key, x => x);
    }

    private static async Task<(bool shouldSave, List<T> data)> ComputeTrend<T>(string cachePath, CandleInterval candleInterval) where T : CachedTrend
    {
        Printer.WriteLine($"Computing {typeof(T)} cache...", White);

        List<T> cachedData = [];

        if (File.Exists(cachePath))
        {
            cachedData = await ParquetService.LoadCachedTrend<T>(cachePath);

            var latestCachedTicks = cachedData.Max(x => x.Key);
            var today = DateTime.UtcNow.Date;

            if (latestCachedTicks >= today.Date.Ticks)
            {
                Printer.AlreadyComputed();
                return (false, cachedData);
            }

            Printer.WriteLine($"🟨 Cache outdated (latest: {latestCachedTicks:yyyy-MM-dd}), updating...", Cyan);
        }
        else
        {
            Printer.WriteLine($"Trend {typeof(T)} cache not found, computing from scratch...", Cyan);
        }

        var candles = await CandlesRepository.GetCandles<TrendCandle>(new BacktestingDetails(candleInterval));
        var indicatorManager = new IndicatorService(null, [IndicatorType.MovingAverage]);
        var totalCandles = candles.Count;
        var result = cachedData.ToDictionary(x => x.Key, x => x) ?? [];
        var remainingCandles = totalCandles - cachedData!.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            var candle = candles[i];
            var candleKey = candleInterval switch
            {
                CandleInterval.Fifteen_Minutes => candle.FifteenMinuteKey,
                CandleInterval.One_Day => candle.DayKey,
                _ => throw new NotImplementedException()
            };

            if (result.ContainsKey(candleKey))
            {
                continue;
            }

            Printer.CalculatingCandles(i, remainingCandles);

            indicatorManager.CalculateRelevantIndicators(candles, i);
            var trend = TrendProfiler.ProfileByMovingAverage(null, candles, i);
            result[candleKey] = (T)Activator.CreateInstance(typeof(T), candle.OpenTime, candle.Indicators.MovingAverage, trend)!;
        }

        return (true, result.Values.OrderBy(x => x.Key).ToList());
    }
}
