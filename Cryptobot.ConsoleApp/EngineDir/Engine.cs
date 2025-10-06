using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.EngineDir;

public class Engine(params Spot[] spots)
{
    private readonly Spot[] _spots = spots;
    private readonly Dictionary<string, IndicatorManager> _indicatorManagers = spots.ToDictionary(x => x.Id, x => new IndicatorManager(x.TradeStrategy));

    public void TradeNewCandle(List<BybitCandle> candles, int currentCandleIndex)
    {
        foreach (var spot in _spots)
        {
            _indicatorManagers[spot.Id].CalculateRelevantIndicators(candles, currentCandleIndex);

            if (spot.TradeStrategy.ShouldOpenTrade(candles, currentCandleIndex, out var position))
            {
                spot.OpenTrade(candles, currentCandleIndex, position!.Value);
            }

            spot.CheckCloseTrades(candles, currentCandleIndex);
        }
    }
}
