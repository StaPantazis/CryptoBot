using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Extensions;

public static class CandleExtensions
{
    public static IEnumerable<(T candle, int i)> AsSeederWithSlice<T>(this List<T> candles, CandleSlice<T> slice) where T : Candle
    {
        for (var i = 0; i < candles.Count; i++)
        {
            slice.AddLiveCandle(candles[i]);
            yield return (candles[i], i);
        }
    }

    public static bool ContainsPrice(this Candle candle, double price) => price >= candle.LowPrice && price <= candle.HighPrice;
}
