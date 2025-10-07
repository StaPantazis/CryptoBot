using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Resources.CachedIndicators;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir;

public class CacheManager
{
    private readonly string _cachePath = Path.Combine(PathHelper.GetCachedIndicatorsOutputPath(), "MacroTrend.parquet");

    public Dictionary<long, CachedMacroTrend> MacroTrendCache { get; private set; } = [];

    public async Task InitializeAsync()
    {
        if (File.Exists(_cachePath))
        {
            var cachedData = await ParquetManager.LoadMacroTrend(_cachePath);
            MacroTrendCache = cachedData.ToDictionary(x => x.OpenTime.Ticks, x => x);
            return;
        }

        Console.WriteLine("⚙️ Computing MacroTrend cache...");
        var computed = await ComputeMacroTrend();

        await ParquetManager.SaveMacroTrend(computed, _cachePath);
        MacroTrendCache = computed.ToDictionary(x => x.OpenTime.Ticks, x => x);
        Console.WriteLine($"✅ Cached {MacroTrendCache.Count:N0} MacroTrend entries.");
    }

    private static async Task<List<CachedMacroTrend>> ComputeMacroTrend()
    {
        var details = new BacktestingDetails(
            Interval: CandleInterval.Fifteen_Minutes,
            Symbol: Constants.SYMBOL_BTCUSDT,
            MarketCategory: Constants.MARKET_PERPETUAL_FUTURES);

        var candles = await Backtester.GetCandles<TrendCandle>(details);
        var result = new List<CachedMacroTrend>(candles.Count);

        var indicatorManager = new IndicatorManager(null, [IndicatorType.MovingAverage]);
        var totalCandles = candles.Count;

        for (var i = 0; i < totalCandles; i++)
        {
            Printer.CalculatingCandles(i, totalCandles);

            indicatorManager.CalculateRelevantIndicators(candles, i);
            var trend = TrendProfiler.ProfileByMovingAverage(null, candles, i);

            result.Add(new CachedMacroTrend(candles[i].OpenTime, candles[i].Indicators.MovingAverage, trend));
        }

        return result;
    }
}
