using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.EngineDir;

public class Engine<T>(CacheService cacheManager, params Spot[] spots) where T : Candle
{
    private readonly CacheService _cacheManager = cacheManager;
    private readonly Spot[] _spots = spots;
    private readonly Dictionary<string, IndicatorService> _indicatorManagers = spots.ToDictionary(x => x.Id, x => new IndicatorService(cacheManager, x.TradeStrategy));

    public void TradeNewCandle(List<T> candles, int currentCandleIndex)
    {
        foreach (var spot in _spots)
        {
            _indicatorManagers[spot.Id].CalculateRelevantIndicators(candles, currentCandleIndex);

            if (spot.TradeStrategy.ShouldOpenTrade(_cacheManager, candles, currentCandleIndex, out var position))
            {
                spot.OpenTrade(candles, currentCandleIndex, position!.Value);
            }

            spot.CheckCloseTrades(candles, currentCandleIndex);
        }
    }
}
