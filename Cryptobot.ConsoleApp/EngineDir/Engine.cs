using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.EngineDir;

public class Engine<T>(CacheService cache, params Spot[] spots) where T : Candle
{
    private readonly Spot[] _spots = spots;
    private readonly Dictionary<string, IndicatorService> _indicatorServicesPerSpot = spots.ToDictionary(x => x.Id, x => new IndicatorService(cache, x.TradeStrategy));
    private readonly DateTime? _filterForDebugging = DateTime.ParseExact("12/08/2025", "dd/MM/yyyy", default);

    public void TradeNewCandle(List<T> candles, int currentCandleIndex, CandleInterval candleInterval)
    {
        if (StopForDebugging(candles, currentCandleIndex))
        {
            return;
        }

        foreach (var spot in _spots)
        {
            _indicatorServicesPerSpot[spot.Id].CalculateRelevantIndicators(candles, currentCandleIndex);

            spot.CheckCloseTrades(candles, currentCandleIndex, candleInterval);

            if (spot.TradeStrategy.ShouldOpenTrade(candles, currentCandleIndex, candleInterval, out var positions) && positions != null)
            {
                foreach (var pos in positions)
                {
                    spot.OpenTrade(candles, currentCandleIndex, pos);
                }
            }
        }
    }

    private bool StopForDebugging(List<T> candles, int currentCandleIndex)
    {
        if (_filterForDebugging != null)
        {
            var candle = candles[currentCandleIndex];

            if (candle.CloseTime < _filterForDebugging)
            {
                return true;
            }
        }

        return false;
    }
}
