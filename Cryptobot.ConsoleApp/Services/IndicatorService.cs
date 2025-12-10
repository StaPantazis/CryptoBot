using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Services;

public class IndicatorService
{
    private readonly CacheService? _cache = null;
    private readonly IndicatorType[] _indicators = [];
    private readonly AiTrendConfiguration? _aiTrendConfig = null;

    public IndicatorService(params IndicatorType[] indicators)
    {
        _indicators = indicators;
    }

    public IndicatorService(AiTrendConfiguration? aiTrendConfig, params IndicatorType[] indicators)
    {
        _aiTrendConfig = aiTrendConfig;
        _indicators = indicators;
    }

    public IndicatorService(CacheService cache, IndicatorType[] indicators)
    {
        _cache = cache;
        _indicators = indicators;
    }

    public IndicatorService(CacheService cache, TradeStrategyBase tradeStrategy)
    {
        _cache = cache;
        _aiTrendConfig = tradeStrategy.AiTrendConfig;
        _indicators = tradeStrategy.RelevantIndicators ?? [];
    }

    public void CalculateRelevantIndicators<T>(T candle, List<T> candles, int currentCandleIndex) where T : Candle
    {
        if (_indicators.Length == 0)
        {
            return;
        }

        foreach (var indicator in _indicators)
        {
            switch (indicator)
            {
                case IndicatorType.MovingAverage:
                    candle.Indicators.MovingAverage = _cache is null
                        ? TrendProfiler.GetMovingAverage(candles, currentCandleIndex)
                        : _cache.MovingAverageTrendCache[candle.DayKey].MovingAverage;

                    candle.Indicators.MovingAverageTrend = _cache is null
                        ? TrendProfiler.ProfileByMovingAverage(_cache, candles, currentCandleIndex, candle)
                        : _cache.MovingAverageTrendCache[candle.DayKey].Trend;

                    break;
                case IndicatorType.AiTrend:
                    candle.Indicators.AiTrend = _cache is null
                        ? TrendProfiler.ProfileAiTrend(candles, currentCandleIndex, _aiTrendConfig!)
                        : _cache.AiTrendCache[candle.GetTimeframeKeyByCandleInterval(_cache.BacktestCandleInterval)].Trend;
                    break;
                default:
                    break;
            }
        }
    }
}
