using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Services;

public class IndicatorService(CacheService? cache, TradeStrategyBase? tradeStrategy)
{
    private readonly CacheService? _cache = cache;
    private readonly IndicatorType[] _indicators = tradeStrategy?.RelevantIndicators ?? [];
    private readonly TrendProfiler? _microTrendProfiler = tradeStrategy?.MicroTrendConfig != null ? new(tradeStrategy.MicroTrendConfig) : null;

    public IndicatorService(CacheService? cache, IndicatorType[] indicators) : this(cache, (TradeStrategyBase?)null)
    {
        _indicators = indicators ?? throw new ArgumentNullException();
    }

    public void CalculateRelevantIndicators<T>(List<T> candles, int currentCandleIndex) where T : Candle
    {
        if (_indicators.Length == 0)
        {
            return;
        }

        var candle = candles[currentCandleIndex];
        CalculateRelevantIndicators(candle, candles, currentCandleIndex);
    }

    public void CalculateRelevantIndicators<T>(T candle, List<T> candles, int currentCandleIndex) where T : Candle
    {
        foreach (var indicator in _indicators)
        {
            switch (indicator)
            {
                case IndicatorType.MovingAverage:
                    candle.Indicators.MovingAverage = _cache is null
                        ? TrendProfiler.GetMovingAverage(candles, currentCandleIndex)
                        : _cache.MacroTrendCache[candle.DayKey].MovingAverage;
                    break;
                case IndicatorType.TrendProfileAI:
                    candle.Indicators.MicroTrend = _microTrendProfiler!.ProfileComplex(candles, currentCandleIndex);
                    break;
                case IndicatorType.MacroTrend:
                    candle.Indicators.MacroTrend = TrendProfiler.ProfileByMovingAverage(_cache, candles, currentCandleIndex, candle);
                    break;
                default:
                    break;
            }
        }
    }
}
