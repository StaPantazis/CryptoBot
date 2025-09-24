using Cryptobot.ConsoleApp.Models;

namespace Cryptobot.ConsoleApp.Bybit.Models;

public class BybitCandle : Candle
{
    public static List<BybitCandle> FromResponse(KlineResponse klineResponse)
    {
        var list = klineResponse.Result.List;
        var candlesticks = new List<BybitCandle>();

        foreach (var item in list)
        {
            var ts = long.Parse(item[0]);
            var openTime = DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime;

            candlesticks.Add(new BybitCandle
            {
                OpenTime = openTime,
                CloseTime = openTime.AddMinutes(1),

                OpenPrice = double.Parse(item[1]),
                HighPrice = double.Parse(item[2]),
                LowPrice = double.Parse(item[3]),
                ClosePrice = double.Parse(item[4]),

                Volume = double.Parse(item[5]),
                QuoteVolume = double.Parse(item[6]),
            });
        }

        return candlesticks;
    }
}
