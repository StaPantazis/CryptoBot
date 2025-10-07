using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.EngineDir;

public class Engine<T>(CacheManager cacheManager, params Spot[] spots) where T : Candle
{
    private readonly CacheManager _cacheManager = cacheManager;
    private readonly Spot[] _spots = spots;
    private readonly Dictionary<string, IndicatorManager> _indicatorManagers = spots.ToDictionary(x => x.Id, x => new IndicatorManager(cacheManager, x.TradeStrategy));

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
