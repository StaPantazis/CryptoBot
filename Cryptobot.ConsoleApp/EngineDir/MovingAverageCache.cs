using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.EngineDir;

public static class MovingAverageCache
{
    public static List<double?> ComputeMovingAverage<T>(List<T> candles, int period) where T : Candle
    {
        var result = new List<double?>(candles.Count);
        double sum = 0;

        for (var i = 0; i < candles.Count; i++)
        {
            sum += candles[i].ClosePrice;

            if (i >= period)
            {
                sum -= candles[i - period].ClosePrice;
            }

            if (i >= period - 1)
            {
                result.Add(sum / period);
            }
            else
            {
                result.Add(null);
            }
        }

        return result;
    }
}
