using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Repositories;
using Cryptobot.ConsoleApp.Resources.CachedIndicators;
using Cryptobot.ConsoleApp.Resources.CachedIndicators.Models;
using Cryptobot.ConsoleApp.Utils;
using static Cryptobot.ConsoleApp.Utils.ConsoleColors;

namespace Cryptobot.ConsoleApp.Services;

public class CacheService
{
    private readonly Dictionary<CandleInterval, Dictionary<long, CachedAiTrend>> _aiTrendPerInterval = [];

    public Dictionary<long, CachedMovingAverageTrend> MovingAverageTrendCache { get; private set; } = [];
    public Dictionary<long, CachedAiTrend> AiTrendCache { get; private set; } = [];

    public async Task InitializeCache()
    {
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.Five_Minutes));
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.Fifteen_Minutes));
        await BybitHistory.Download(new BacktestingDetails(CandleInterval.One_Day));

        await ComputeMovingAverageTrend();
        await ComputeTrendProfilerAiTrends();

        await CandleValidatorService.ValidateStoredResources();
    }

    public void SetBacktestInterval(BacktestingDetails details) => AiTrendCache = _aiTrendPerInterval[details.Interval];

    private async Task ComputeMovingAverageTrend()
    {
        var movingAverageCachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), "MovingAverage.parquet");
        var (shouldSave, cachedData) = await CheckFetchCompleteData(movingAverageCachePath, ParquetService.LoadMovingAverageTrend<CachedMovingAverageTrend>);

        if (shouldSave)
        {
            cachedData = await CalculateTrendCandles(
                candleInterval: CandleInterval.One_Day,
                cachedData: cachedData,
                indicatorService: new IndicatorService(IndicatorType.MovingAverage),
                getInstanceArgs: candle => [candle.OpenTime, candle.Indicators.MovingAverage, candle.Indicators.MovingAverageTrend]);

            await ParquetService.SaveMovingAverageTrend(cachedData, movingAverageCachePath);
            Printer.EmptyLine();
        }

        MovingAverageTrendCache = cachedData.ToDictionary(x => x.Key, x => x);
    }

    private async Task ComputeTrendProfilerAiTrends()
    {
        CandleInterval[] intervals = [CandleInterval.Five_Minutes, CandleInterval.Fifteen_Minutes];
        var profiles = Enum.GetValues<AiTrendProfile>();

        foreach (var candleInterval in intervals)
        {
            foreach (var profile in profiles)
            {
                var aiTrendCachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), CachedAiTrend.CachedFileName(profile, candleInterval));

                Func<string, Task<List<CachedAiTrend>>> loader = profile switch
                {
                    AiTrendProfile.Default => path => ParquetService.LoadCachedAiTrend<CachedAiTrend_Default>(path).ContinueWith(x => x.Result.Cast<CachedAiTrend>().ToList()),
                    AiTrendProfile.Balanced => path => ParquetService.LoadCachedAiTrend<CachedAiTrend_Balanced>(path).ContinueWith(x => x.Result.Cast<CachedAiTrend>().ToList()),
                    AiTrendProfile.Conservative => path => ParquetService.LoadCachedAiTrend<CachedAiTrend_Conservative>(path).ContinueWith(x => x.Result.Cast<CachedAiTrend>().ToList()),
                    AiTrendProfile.Aggressive => path => ParquetService.LoadCachedAiTrend<CachedAiTrend_Aggressive>(path).ContinueWith(x => x.Result.Cast<CachedAiTrend>().ToList()),
                    _ => throw new NotImplementedException(),
                };

                var (shouldSave, cachedData) = await CheckFetchCompleteData(aiTrendCachePath, loader);

                if (shouldSave)
                {
                    cachedData = await CalculateTrendCandles(
                        candleInterval: candleInterval,
                        cachedData: cachedData,
                        indicatorService: new IndicatorService(AiTrendConfiguration.Create(profile), IndicatorType.AiTrend),
                        getInstanceArgs: candle => [candle.OpenTime, candle.Indicators.AiTrend!.Value, candleInterval]);

                    await ParquetService.SaveAiTrend(cachedData, aiTrendCachePath);
                }

                _aiTrendPerInterval[candleInterval] = cachedData.ToDictionary(x => x.Key, x => x);
            }
        }
    }

    private static async Task<(bool shouldSave, List<T> cachedData)> CheckFetchCompleteData<T>(string cachePath, Func<string, Task<List<T>>> loadData) where T : CachedTrend
    {
        Printer.WriteLine($"Computing {typeof(T).Name} cache...", White);

        List<T> cachedData = [];

        if (File.Exists(cachePath))
        {
            cachedData = await loadData(cachePath);

            var latestCachedTicks = cachedData.Max(x => x.OpenDateTime.Date.Ticks);

            // TODO :: This should work for the candle interval. Right now it works per day
            var today = DateTime.UtcNow.Date.AddDays(-1);

            if (latestCachedTicks >= today.Date.Ticks)
            {
                Printer.AlreadyComputed();
                return (false, cachedData);
            }

            Printer.WriteLine($"🟨 Cache outdated (latest: {latestCachedTicks:yyyy-MM-dd}), updating...", Cyan);
        }
        else
        {
            Printer.WriteLine($"Trend {typeof(T).Name} cache not found, computing from scratch...", Cyan);
        }

        return (true, []);
    }

    private static async Task<List<T>> CalculateTrendCandles<T>(CandleInterval candleInterval, List<T> cachedData, IndicatorService indicatorService, Func<TrendCandle, object?[]> getInstanceArgs)
        where T : CachedTrend
    {
        var candles = await CandlesRepository.GetCandles<TrendCandle>(new BacktestingDetails(candleInterval));
        var totalCandles = candles.Count;
        var result = cachedData.ToDictionary(x => x.Key, x => x) ?? [];
        var remainingCandles = totalCandles - cachedData.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            var candle = candles[i];
            var candleKey = candleInterval switch
            {
                CandleInterval.Five_Minutes => candle.FiveMinuteKey,
                CandleInterval.Fifteen_Minutes => candle.FifteenMinuteKey,
                CandleInterval.One_Day => candle.DayKey,
                _ => throw new NotImplementedException()
            };

            if (result.ContainsKey(candleKey))
            {
                continue;
            }

            Printer.CalculatingCandles(i, remainingCandles);

            indicatorService.CalculateRelevantIndicators(candles, i);
            result[candleKey] = (T)Activator.CreateInstance(typeof(T), getInstanceArgs(candle))!;
        }

        return result.Values.OrderBy(x => x.Key).ToList();
    }
}
