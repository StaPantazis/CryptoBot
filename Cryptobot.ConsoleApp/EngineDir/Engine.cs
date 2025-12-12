using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.EngineDir;

public class Engine<T>(CacheService cache, params Spot[] spots) where T : Candle
{

    private readonly Spot[] _spots = spots;
    private readonly Dictionary<string, IndicatorService> _indicatorServicesPerSpot = spots.ToDictionary(x => x.Id, x => new IndicatorService(cache, x.TradeStrategy));
    private readonly DateTime? _filterForDebugging = null;// DateTime.ParseExact("12/08/2025", "dd/MM/yyyy", default);

    public void TradeLive(CandleSlice<T> slice)
    {
        if (StopForDebugging(slice.LiveCandle))
        {
            return;
        }

        foreach (var spot in _spots)
        {
            _indicatorServicesPerSpot[spot.Id].CalculateRelevantIndicators(slice);

            spot.CheckCloseTrades(slice);

            if (spot.TradeStrategy.ShouldOpenTrade(slice, out var positions) && positions != null)
            {
                foreach (var pos in positions)
                {
                    spot.OpenTrade(slice, pos);
                }
            }
        }
    }

    private bool StopForDebugging(T liveCandle) => _filterForDebugging != null && liveCandle.CloseTime < _filterForDebugging;
}
