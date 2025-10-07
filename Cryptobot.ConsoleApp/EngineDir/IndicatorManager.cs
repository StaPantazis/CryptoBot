﻿using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir;

public class IndicatorManager(CacheManager? cacheManager, TradeStrategyBase? tradeStrategy)
{
    private readonly CacheManager? _cacheManager = cacheManager;
    private readonly IndicatorType[] _indicators = tradeStrategy?.RelevantIndicators ?? [];
    private readonly TrendProfiler? _microTrendProfiler = tradeStrategy != null ? new(tradeStrategy.MicroTrendConfiguration) : null;

    public IndicatorManager(CacheManager? cacheManager, IndicatorType[] indicators) : this(cacheManager, (TradeStrategyBase?)null)
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

        foreach (var indicator in _indicators)
        {
            switch (indicator)
            {
                case IndicatorType.MovingAverage:
                    candle.Indicators.MovingAverage = TrendProfiler.GetMovingAverage(candles, currentCandleIndex, 30);
                    break;
                case IndicatorType.MicroTrend:
                    candle.Indicators.MicroTrend = _microTrendProfiler!.ProfileComplex(candles, currentCandleIndex);
                    break;
                case IndicatorType.MacroTrend:
                    candle.Indicators.MacroTrend = TrendProfiler.ProfileByMovingAverage(_cacheManager, candles, currentCandleIndex, candle);
                    break;
                default:
                    break;
            }
        }
    }
}
