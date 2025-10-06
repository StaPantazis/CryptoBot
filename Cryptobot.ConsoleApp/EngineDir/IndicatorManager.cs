using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir;

public class IndicatorManager(TradeStrategy tradeStrategy)
{
    private readonly IndicatorType[] _indicators = tradeStrategy.RelevantIndicators;
    private readonly TrendProfiler? _trendProfiler = tradeStrategy is TrendTradeStrategy t ? new(t.TrendConfiguration) : null;

    public void CalculateRelevantIndicators<T>(List<T> candles, int currentCandleIndex) where T : Candle
    {
        var candle = candles[currentCandleIndex];

        foreach (var indicator in _indicators)
        {
            switch (indicator)
            {
                case IndicatorType.MovingAverage:
                    candle.Indicators.MovingAverage = GetMovingAverage(candles, currentCandleIndex, 30);
                    break;
                case IndicatorType.Trend:
                    candle.Indicators.Trend = _trendProfiler!.Profile(candles, currentCandleIndex);
                    break;
                default:
                    break;
            }
        }
    }

    private static double? GetMovingAverage<T>(List<T> candles, int currentCandleIndex, int window) where T : Candle
    {
        if (candles == null || candles.Count == 0)
        {
            return null;
        }
        else if (window < 1)
        {
            throw new ArgumentException("Window must be >= 1", nameof(window));
        }
        else if (currentCandleIndex < window - 1)
        {
            return null;
        }

        // Select last 'window' candles ending at the current index
        var slice = candles.Skip(currentCandleIndex - window + 1).Take(window);
        return slice.Average(c => c.ClosePrice);
    }
}
