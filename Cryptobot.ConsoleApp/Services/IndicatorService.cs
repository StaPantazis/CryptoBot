using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

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

    public int SliceSizeBasedOnIndicators => _indicators.Contains(IndicatorType.MovingAverage)
        ? Constants.MOVING_AVERAGE_WINDOW
        : (_aiTrendConfig?.Window ?? 0);

    public void CalculateRelevantIndicators<T>(CandleSlice<T> slice) where T : Candle
    {
        if (_indicators.Length == 0)
        {
            return;
        }

        var liveCandle = slice.LiveCandle;

        foreach (var indicator in _indicators)
        {
            switch (indicator)
            {
                case IndicatorType.MovingAverage:
                    liveCandle.Indicators.MovingAverage = _cache is null
                        ? TrendProfiler.GetMovingAverage(slice)
                        : _cache.MovingAverageTrendCache[liveCandle.DayKey].MovingAverage;

                    liveCandle.Indicators.MovingAverageTrend = _cache is null
                        ? TrendProfiler.ProfileByMovingAverage(_cache, slice)
                        : _cache.MovingAverageTrendCache[liveCandle.DayKey].Trend;

                    break;
                case IndicatorType.AiTrend:
                    liveCandle.Indicators.AiTrend = _cache is null
                        ? TrendProfiler.ProfileAiTrend(slice, _aiTrendConfig!)
                        : _cache.AiTrendCache[liveCandle.GetTimeframeKeyByCandleInterval(_cache.BacktestCandleInterval)].Trend;
                    break;
                default:
                    break;
            }
        }
    }
}
